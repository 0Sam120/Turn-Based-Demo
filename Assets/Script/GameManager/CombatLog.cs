using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatLog : MonoBehaviour
{

    public GameObject logPanel;
    public Button toggleButton;
    public ScrollRect scrollRect;
    public Transform logContent;
    public GameObject logEntryPrefab;
    private List<GameObject> logEntries = new List<GameObject>();

    public int maxLogEntries = 100;
    public bool autoScrollToBottom = true;
    private bool isExpanded = true;

    void Awake()
    {
        // Set up the toggle button
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleLog);
        }

        // Initialize log as expanded
        SetLogVisibility(!isExpanded);
    }

    public void ToggleLog()
    {
        isExpanded = !isExpanded;
        SetLogVisibility(isExpanded);
    }

    private void SetLogVisibility(bool visible)
    {
        if (logPanel != null)
        {
            logPanel.SetActive(visible);
        }

        // Update button text
        if (toggleButton != null)
        {
            TextMeshProUGUI buttonText = toggleButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = visible ? "Hide Log" : "Show Log";
            }
        }
    }

    public void AddLogEntry(string message)
    {
        // Create new log entry
        GameObject newEntry = Instantiate(logEntryPrefab, logContent);
        TextMeshProUGUI textComponent = newEntry.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            textComponent.text = message;
        }

        logEntries.Add(newEntry);

        // Remove old entries if we exceed max count
        while (logEntries.Count > maxLogEntries)
        {
            GameObject oldEntry = logEntries[0];
            logEntries.RemoveAt(0);
            DestroyImmediate(oldEntry);
        }

        // Auto-scroll to bottom if enabled
        if (autoScrollToBottom && scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(logContent.GetComponent<RectTransform>());

            // Scroll to bottom after rebuild
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }

    public void ClearLog()
    {
        foreach (GameObject entry in logEntries)
        {
            if (entry != null)
            {
                DestroyImmediate(entry);
            }
        }
        logEntries.Clear();
    }

    public void LogAttack(string attackerName, string targetName, int rollValue, int damage)
    {
        string message = $"[{attackerName}] attacked [{targetName}] and rolled [{rollValue}]. [{targetName}] took [{damage}] damage.";
        AddLogEntry(message);
    }

    public void LogMiss(string attackerName, string targetName, int rollValue)
    {
        string message = $"[{attackerName}] attacked [{targetName}] and rolled [{rollValue}]. Attack missed!";
        AddLogEntry(message);
    }

    public void LogCriticalHit(string attackerName, string targetName, int rollValue, int damage)
    {
        string message = $"[{attackerName}] critically hit [{targetName}] with a roll of [{rollValue}]! [{targetName}] took [{damage}] damage!";
        AddLogEntry(message);
    }

    public void LogHeal(string healerName, string targetName, int healAmount)
    {
        string message = $"[{healerName}] healed [{targetName}] for [{healAmount}] HP.";
        AddLogEntry(message);
    }

    public void LogStatusEffect(string casterName, string targetName, string effectName)
    {
        string message = $"[{casterName}] applied [{effectName}] to [{targetName}].";
        AddLogEntry(message);
    }

    public void LogUnitDeath(string unitName)
    {
        string message = $"[{unitName}] has been defeated!";
        AddLogEntry(message);
    }

    public void LogTurnStart(string unitName)
    {
        string message = $"--- [{unitName}]'s turn begins ---";
        AddLogEntry(message);
    }

    public void LogRoundStart(int roundNumber)
    {
        string message = $"=== Starting Round {roundNumber} START ===";
        AddLogEntry(message);
    }

    public void LogBattleStart()
    {
        string message = "=== BATTLE STARTED ===";
        AddLogEntry(message);
    }

    public void LogBattleEnd(string winner)
    {
        string message = $"=== BATTLE ENDED - [{winner}] VICTORIOUS ===";
        AddLogEntry(message);
    }
}
