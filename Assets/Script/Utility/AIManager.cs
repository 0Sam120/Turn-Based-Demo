using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using static TurnManager;


public enum AIState
{
    Idle,
    Evaluate,
    Moving,
    Attacking,
    SeekingCover,
    Waiting,
    EndTurn
}

public enum AIType
{
    Aggressive,
    Defensive,
    Cowardly,
    Opportunistic,
    Support
}

public class AIManager : MonoBehaviour
{
    [SerializeField] GridMap targetGrid;
    private Coroutine coroutine;
    CharacterTurn characterTurn;
    Character thisUnit;
    Pathfinder pathfinder;
    GridObject gridObj;
    AnimatorProxy proxy;
    Animator animator;
    Vector2Int currentPos;
    AIType currentType;
    AIState currentState;
    int optimalRange;
    int maxRange;
    int minRange;

    private void Awake()
    {
        pathfinder = new Pathfinder(targetGrid);
    }

    public void UpdateAI()
    {
        switch (currentState)
        {
            case AIState.Evaluate:
                currentState = WellnessCheck();
                UpdateAI(); // run again immediately with new state
                break;

            case AIState.SeekingCover:
                SeekCover();
                break;

            case AIState.Attacking:
                PerformAttack();
                break;

            case AIState.Moving:
                MoveToPosition();
                break;

            case AIState.Idle:
                currentState = AIState.EndTurn;
                break;

            case AIState.EndTurn:
                TurnManager.Instance.EndCurrentUnitTurn();
                break;
        }
    }


    public void HandleAITurn(Character unit)
    {
        if (TurnManager.Instance.state != GameState.EnemyTurn)
        {
            Debug.LogWarning("Tried to run AI turn outside of EnemyTurn phase!");
            return;
        }

        thisUnit = unit;
        currentState = AIState.Evaluate;
        characterTurn = thisUnit.GetComponent<CharacterTurn>();
        gridObj = thisUnit.GetComponent<GridObject>();
        animator = thisUnit.GetComponentInChildren<Animator>();
        proxy = new AnimatorProxy(animator, this);
        currentPos = gridObj.positionOnGrid;
        maxRange = thisUnit.atkRange;
        minRange = 1;
        optimalRange = Mathf.CeilToInt(maxRange / 2);
        UpdateAI();
    }

    private AIState WellnessCheck()
    {
        if (!CanIAct(thisUnit, 1))
            return AIState.Idle;

        if (thisUnit.HP < thisUnit.maxHP * 0.2 && CanIAct(thisUnit, 2))
        {
            Debug.Log("Unit HP is low, seeking cover.");
            return AIState.SeekingCover;
        }

        var enemies = GetEnemiesWithDistance();

        if (enemies.Count > 0)
        {
            var nearest = enemies.OrderBy(ed => ed.distance).First().enemy.GetComponent<GridObject>().positionOnGrid;

            if (IsInRange(currentPos, nearest, minRange) && CanIAct(thisUnit, 2))
            {
                Debug.Log("Enemy too close, disengaging.");
                return AIState.Moving;
            }

            if (IsInRange(currentPos, nearest, optimalRange) && CanIAct(thisUnit, 2))
            {
                return AIState.Attacking;
            }
            else
            {
                return AIState.Moving;
            }
        }
        else if (CanIAct(thisUnit, 2))
        {
            Debug.Log("No enemies in range, moving to closest enemy.");
            return AIState.Moving;
        }

        return AIState.Idle;
    }


    private void SeekCover()
    {
        IList list = GetEnemiesWithDistance();
        for (int i = 0; i < list.Count; i++)
        {
            Character otherUnit = (Character)list[i];
            if (otherUnit != thisUnit)
            {
                Vector2Int unitPos = currentPos;
                Vector2Int targetPos = otherUnit.GetComponent<GridObject>().positionOnGrid;
                Debug.Log("Enemy Unit Found: " + otherUnit.name + " at position: " + targetPos);
                if (IsInRange(unitPos, targetPos, otherUnit.atkRange))
                {
                    Debug.Log("In Range of Enemy: " + otherUnit.name);
                    Debug.Log("Seeking cover from " + otherUnit.name);
                    MoveToPosition();
                }
            }
        }
    }

    private void MoveToPosition()
    {
        var path = pathfinder.FindPath(currentPos, CalculateBestMovePosition());
        bool canSpend = CanIAct(thisUnit, 2);
        bool pathIsValid = thisUnit.GetComponent<UnitMovement>().PathIsValid(path);

        if (!pathIsValid || !canSpend)
        {
            Debug.Log("Cannot move to position, either path is invalid or not enough momentum.");
            return;
        }

        characterTurn.SpendMomentum(2);
        Debug.Log($"Spent momentum:  {characterTurn.Momentum} remaining");
        thisUnit.GetComponent<UnitMovement>().Move(path);
        proxy.WaitUntilAnimationStops(() =>
        {
            currentPos = thisUnit.GetComponent<GridObject>().positionOnGrid;
            currentState = AIState.Evaluate;
            StartCoroutine(DelayedUpdateAI());
        }
        );
    }

    private void PerformAttack()
    {
        var enemies = GetEnemiesWithDistance();
        var enemiesInRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
        if (enemiesInRange.Count > 0 && CanIAct(thisUnit, 2))
        {
            characterTurn.SpendMomentum(2);
            Debug.Log("Spent momentum" + characterTurn.Momentum);
            Character targetEnemy = enemiesInRange.OrderBy(ed => ed.distance).FirstOrDefault().enemy; // For simplicity, target the first enemy in range
            Debug.Log("Attacking enemy: " + targetEnemy.name);
            // Add attack logic here, e.g., targetEnemy.TakeDamage(thisUnit.atkDamage);
            int total = thisUnit.RollToHit();
            thisUnit.GetComponent<AttackComponent>().AttackPosition(targetEnemy.GetComponent<GridObject>(), total);
            Debug.Log(proxy == null ? "Proxy is NULL" : "Proxy is fine");
            proxy.WaitUntilAnimationStops(() =>
            {
                currentState = AIState.Evaluate;
                StartCoroutine(DelayedUpdateAI()); // Delay to allow for attack animation
            }
            );
        }
        else
        {
            Debug.Log("No enemies in range to attack.");
        }
    }

    Vector2Int CalculateBestMovePosition()
    {
        Vector2Int bestPos = currentPos;
        int bestScore = int.MinValue;

        List<Vector2Int> walkableTiles = GetWalkableTilesInRange(thisUnit, (int)thisUnit.MaxMoveSpeed);
        List<PathNode> walkableNodes = new List<PathNode>();
        var enemies = GetEnemiesWithDistance();
        var inRange = enemies.Where(ed => ed.distance <= thisUnit.atkRange).ToList();
        var closest = enemies.OrderBy(ed => ed.distance).FirstOrDefault().enemy;

        if (enemies.Count == 0)
            return currentPos; // no enemies at all, shouldn't happen under normal conditions

        Character targetEnemy = null;

        if (inRange.Count > 0)
        {
            targetEnemy = inRange.OrderBy(ed => ed.distance).FirstOrDefault().enemy;
        }
        else
        {
            targetEnemy = closest;
        }

        Vector2Int enemyPos = targetEnemy.GetComponent<GridObject>().positionOnGrid;
        int distanceToEnemy = Mathf.Abs(currentPos.x - enemyPos.x) + Mathf.Abs(currentPos.y - enemyPos.y);

        bool pursuitMode = distanceToEnemy > thisUnit.atkRange;

        foreach (Vector2Int tile in walkableTiles)
        {
            List<Vector2Int> path = pathfinder.FindPath(currentPos, tile);
            if (path == null || path.Count == 0) continue;

            int score = ScoreTile(tile, targetEnemy, pursuitMode);

            if (score > bestScore)
            {
                bestScore = score;
                bestPos = tile;
            }
        }

        return bestPos;
    }

    List<Vector2Int> GetWalkableTilesInRange(Character unit, int moveRange)
    {
        Vector2Int startPos = unit.GetComponent<GridObject>().positionOnGrid;
        List<Vector2Int> walkableTiles = new List<Vector2Int>();

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                Vector2Int tilePos = new Vector2Int(startPos.x + x, startPos.y + y);

                if (!targetGrid.CheckBoundry(tilePos)) continue;
                if (!targetGrid.CheckWalkable(tilePos)) continue;

                int distance = Mathf.Abs(x) + Mathf.Abs(y);
                if (distance <= moveRange)
                {
                    walkableTiles.Add(tilePos);
                }
            }
        }

        return walkableTiles;
    }


    public List<Character> GetAllEnemiesOnMap()
    {
        List<Character> enemies = new List<Character>();

        foreach (Character otherUnit in UnitRegistry.AllUnits)
        {
            if (otherUnit.team != thisUnit.team && otherUnit != thisUnit)
            {
                Vector2Int unitPos = currentPos;
                Vector2Int targetPos = otherUnit.GetComponent<GridObject>().positionOnGrid;
                Debug.Log(Equals(otherUnit, thisUnit) ? "Same Unit" : "Enemy Unit Found: " + otherUnit.name + " at position: " + targetPos);

                enemies.Add(otherUnit);
            }
        }

        return enemies;
    }

    List<(Character enemy, int distance)> GetEnemiesWithDistance()
    {
        var enemies = new List<(Character, int)>();
        foreach (var e in GetAllEnemiesOnMap())
        {
            int dist = ManhattanDistance(currentPos, e.GetComponent<GridObject>().positionOnGrid); // or pathfinding cost
            enemies.Add((e, dist));
        }
        return enemies;
    }


    int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public bool IsInRange(Vector2Int unitPos, Vector2Int targetPos, int range)
    {
        int distance = Mathf.Abs(unitPos.x - targetPos.x) + Mathf.Abs(unitPos.y - targetPos.y);
        return distance <= range;
    }

    internal bool CanIAct(Character unit, int momentumCost)
    {
        if (unit.GetComponent<CharacterTurn>().Momentum < momentumCost)
        {
            Debug.Log("Unit has no momentum to act.");
            return false;
        }

        return true;
    }

    private int ScoreTile(Vector2Int tilePos, Character targetUnit, bool pursuitMode = false)
    {
        int score = 0;
        int randomMod = UnityEngine.Random.Range(-5, 5);
        Vector2Int targetPos = targetUnit.GetComponent<GridObject>().positionOnGrid;

        int distance = Mathf.Abs(tilePos.x - targetPos.x) + Mathf.Abs(tilePos.y - targetPos.y);

        if (pursuitMode)
        {
            // In pursuit, we want to *approach* the optimal range, not avoid it
            // So we reverse the logic: lower distance → better score (but still aim for optimal)
            score -= Mathf.Abs(distance - optimalRange);

            // Optional: encourage ending up near, but not *adjacent* to, the enemy
            if (distance < minRange)
                score -= 5; // gentle nudge instead of a hard slap
        }
        else
        {
            if (distance < minRange)
                score -= 20; // nope, back off
            else if (distance == optimalRange)
                score += 10;
            else
                score -= Mathf.Abs(distance - optimalRange);
        }

        score += randomMod;
        return score;
    }

    private IEnumerator DelayedUpdateAI()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        UpdateAI();
    }

}