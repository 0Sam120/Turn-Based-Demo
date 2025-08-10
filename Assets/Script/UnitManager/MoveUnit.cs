using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveUnit : MonoBehaviour
{
    [SerializeField] private GridMap grid; // Implements IGridMap
    [SerializeField] private GridRenderer targetRenderer;

    private Pathfinder pathfinder;

    private void Awake()
    {
        pathfinder = new Pathfinder(grid); // No GetComponent nonsense — clean instantiation
    }

    public void CheckWalkableTerrain(Character targetCharacter)
    {
        GridObject gridObject = targetCharacter.GetComponent<GridObject>();
        Vector2Int origin = gridObject.positionOnGrid;
        float maxCost = targetCharacter.MaxMoveSpeed;

        // Set up settings to simulate a range scan without a goal
        PathfindingSettings settings = new PathfindingSettings
        {
            MaxSearchCost = maxCost,
            UseHeuristic = false // simulate Dijkstra-like spread
        };

        List<Vector2Int> reachableTiles = FloodFillWalkable(origin, settings);

        targetRenderer.Hide();
        targetRenderer.fieldHighlight(reachableTiles);
    }

    public List<Vector2Int> GetPath(Vector2Int from, Vector2Int to)
    {
        // from = origin, to = target destination
        var path = pathfinder.FindPath(from, to);

        return path is { Count: > 0 } ? path : null;
    }

    /// <summary>
    /// Runs a Dijkstra-style floodfill to get all tiles reachable within movement range.
    /// </summary>
    private List<Vector2Int> FloodFillWalkable(Vector2Int origin, PathfindingSettings settings)
    {
        var openSet = new SortedSet<NodeRecord>(new NodeRecordComparer());
        var visited = new HashSet<Vector2Int>();
        var costSoFar = new Dictionary<Vector2Int, float>();
        var result = new List<Vector2Int>();

        var startRecord = new NodeRecord
        {
            position = origin,
            gCost = 0,
            hCost = 0,
            parent = null
        };

        openSet.Add(startRecord);
        costSoFar[origin] = 0;

        while (openSet.Count > 0)
        {
            var current = openSet.Min;
            openSet.Remove(current);

            if (visited.Contains(current.position))
                continue;

            visited.Add(current.position);
            result.Add(current.position);

            foreach (var neighbor in GetNeighbors(current.position))
            {
                if (!grid.CheckBoundry(neighbor) || !grid.CheckWalkable(neighbor))
                    continue;

                if (settings?.AdditionalWalkabilityCheck?.Invoke(neighbor) == false)
                    continue;

                float moveCost = settings?.MovementCostOverride?.Invoke(neighbor) ?? 1f;
                float newCost = current.gCost + moveCost;

                if (newCost > (settings?.MaxSearchCost ?? float.MaxValue))
                    continue;

                if (costSoFar.TryGetValue(neighbor, out float existingCost) && newCost >= existingCost)
                    continue;

                costSoFar[neighbor] = newCost;

                var neighborRecord = new NodeRecord
                {
                    position = neighbor,
                    gCost = newCost,
                    hCost = 0,
                    parent = current
                };

                openSet.Add(neighborRecord);
            }
        }

        return result;
    }


    private static readonly Vector2Int[] cardinalDirections = new Vector2Int[]
    {
        new Vector2Int(0, 1), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(-1, 0)
    };

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        foreach (var dir in cardinalDirections)
            yield return pos + dir;
    }
}
