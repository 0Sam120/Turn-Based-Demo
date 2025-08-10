using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
    Setup,
    PlayerTurn,
    EnemyTurn,
    CheckWinCondition,
    CombatEnd,
    Awaiting,
    Busy
}

public class TurnManager : MonoBehaviour
{

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private TextMeshProUGUI resultTitle;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;

    public List<Character> ActiveUnits = new List<Character>();
    public List<Character> PlayerTeam = new List<Character>();
    public List<Character> EnemyTeam = new List<Character>();

    public static TurnManager Instance { get; private set; }

    public Character currentUnit;
    public CommandMenu commandMenu;
    public AIManager AI;
    public GameState state;
    public int combatRound = 0;
    public int currentInitiativeIndex = 0;
    private bool isEndingTurn = false;
    AnimatorProxy proxy;
    CombatLog combatLog;


    private void Awake()
    {
        combatLog = GetComponent<CombatLog>();
        commandMenu = GetComponent<CommandMenu>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        Character.OnCharacterDeath += OnCharacterDied;
    }

    private void OnDisable()
    {
        Character.OnCharacterDeath -= OnCharacterDied;
    }

    void Start()
    {
        state = GameState.Setup;
        new WaitForSeconds(3f);
        combatLog.LogBattleStart();
        StartCombat();
    }

    public void StartCombat()
    {
        ScanForActiveUnits();
        AssignUnitsToTeams();
        RollInitiative();
        IncrementRounds();
    }

    void ScanForActiveUnits()
    {
        ActiveUnits = UnitRegistry.AllUnits.Where(unit => unit.IsAlive()).ToList();
        Debug.Log($"Found {ActiveUnits.Count} active units for combat");
    }

    void AssignUnitsToTeams()
    {
        PlayerTeam = ActiveUnits.Where(unit => unit.team == Team.Player).ToList();
        EnemyTeam = ActiveUnits.Where(unit => unit.team == Team.Enemy).ToList();
        Debug.Log($"Player Team: {PlayerTeam.Count} units, Enemy Team: {EnemyTeam.Count} units");
    }

    void RollInitiative()
    {
        foreach(var unit in ActiveUnits)
        {
            unit.Initiative = Random.Range(1, 21) + unit.InitiativeMod; // Roll 1d20 + Initiative Modifier
            Debug.Log($"{unit.Name} rolled initiative: {unit.Initiative}");
        }

        ActiveUnits = ActiveUnits.OrderByDescending(unit => unit.Initiative).ToList();

        Debug.Log("Initiative Order:");
        for(int i = 0; i < ActiveUnits.Count; i++)
        {
            Debug.Log($"{i + 1}. {ActiveUnits[i].Name} (Initiative: {ActiveUnits[i].Initiative})");
        }
    }

    void ProcessCurrentUnit()
    {
        combatLog.LogTurnStart(currentUnit.name);

        if (currentUnit.team == Team.Player)
        {
            state = GameState.PlayerTurn;
            HandlePlayerTurn();
        }
        else
        {
            state = GameState.EnemyTurn;
            HandleEnemyTurn();
        }
    }

    public static class UnitRegistry
    {
        public static readonly List<Character> AllUnits = new List<Character>();

        public static void Register(Character unit) => AllUnits.Add(unit);
        public static void Deregister(Character unit) => AllUnits.Remove(unit);
    }

    private void OnCharacterDied(Character deadCharacter)
    {
        // Deregister from unit registry
        UnitRegistry.Deregister(deadCharacter);

        // Update team counts
        if (deadCharacter.team == Team.Player)
        {
            PlayerTeam.Remove(deadCharacter);
        }
        else if (deadCharacter.team == Team.Enemy)
        {
            EnemyTeam.Remove(deadCharacter);
        }

        // Handle turn logic...
        if (currentUnit == deadCharacter)
        {
            EndCurrentUnitTurn();
        }

        CheckForBattleEnd();
    }

    void IncrementRounds()
    {
        combatRound++;
        currentInitiativeIndex = 0;

        combatLog.LogRoundStart(combatRound);

        if (ActiveUnits.Count > 0)
        {
            currentUnit = ActiveUnits[currentInitiativeIndex];
            ProcessCurrentUnit();
        }
    }

    public void HandlePlayerTurn()
    {
        currentUnit.GetComponent<CharacterTurn>().GrantTurn();
        Debug.Log("Player's turn started");
    }

    public void HandleEnemyTurn()
    {
        if (currentUnit == null)
        {
            Debug.LogError("Current unit is null in HandleEnemyTurn");
            return;
        }
        var ai = currentUnit.GetComponent<AIManager>();
        if (ai == null)
        {
            Debug.LogError("AI is null in HandleEnemyTurn");
            return;
        }

        ai.GetComponent<CharacterTurn>().GrantTurn();
        ai.HandleAITurn(currentUnit);
    }

    public void CheckForBattleEnd()
    {
        // Check if either team has no active units left
        if (PlayerTeam.Count == 0 || EnemyTeam.Count == 0)
        {
            state = GameState.CombatEnd;
            combatLog.LogBattleEnd(PlayerTeam.Count > 0 ? "Player" : "Enemy");
        }
    }

    private void ForceEndTurn()
    {
        var clear = FindAnyObjectByType<ClearUtility>();

        StopAllCoroutines();

        if(currentUnit != null)
        {
            currentUnit = null;
        }

        clear.FullClear();
        commandMenu.ClosePanel();
    }

    private void ShowResultScreen(string winner)
    {
        
        // Setup UI
        resultScreen.SetActive(true);
        resultTitle.text = winner == "Player" ? "VICTORY!" : "DEFEAT";

        // Setup buttons
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartBattle);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitToMenu);

        // Optional: Play victory/defeat sound
        // AudioManager.PlaySound(winner == "Player" ? victorySound : defeatSound);
    }

    public void EndCurrentUnitTurn()
    {
        if (isEndingTurn) return;
        isEndingTurn = true;

        proxy = new AnimatorProxy(currentUnit.GetComponentInChildren<Animator>(), this);
        proxy.WaitUntilAnimationStops(() =>
        {
            commandMenu.ClosePanel();
            state = GameState.Awaiting;
            Debug.Log($"{currentUnit.name}'s turn ended");

            ActiveUnits.RemoveAll(unit => !unit.IsAlive());

            CheckForBattleEnd();

            currentInitiativeIndex++;
            if (currentInitiativeIndex >= ActiveUnits.Count)
            {
                isEndingTurn = false;
                IncrementRounds();
            }
            else
            {
                currentUnit = ActiveUnits[currentInitiativeIndex];
                isEndingTurn = false;
                ProcessCurrentUnit();
            }
        }
        );
    }

    private void QuitToMenu()
    {
        Application.Quit();
    }

    private void RestartBattle()
    {
        // Hide result screen
        resultScreen.SetActive(false);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
