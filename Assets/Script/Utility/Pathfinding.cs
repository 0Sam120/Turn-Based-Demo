using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

// Represents a node used for pathfinding (separate from the world Node class)
public struct PathNode
{
    public int x;
    public int y;
    public bool walkable;

    public PathNode(int x, int y, bool walkable)
    {
        this.x = x;
        this.y = y;
        this.walkable = walkable;
    }

    public Vector2Int Position => new Vector2Int(x, y);
}

internal class NodeRecord
{
    public Vector2Int position;
    public float gCost;
    public float hCost;
    public float FCost => gCost + hCost;

    public NodeRecord parent;
}



public class NodeRecordComparer : IComparer<NodeRecord>
{
    int IComparer<NodeRecord>.Compare(NodeRecord x, NodeRecord y)
    {
        int result = x.FCost.CompareTo(y.FCost);
        if (result == 0)
            result = x.position.GetHashCode().CompareTo(y.position.GetHashCode());
        return result;
    }
}

public interface IGridMap
{
    bool CheckWalkable(Vector2Int position);
    bool CheckBoundry(Vector2Int position);
}

public class PathfindingSettings
{
    /// <summary>
    /// Allows diagonal movement (NW, NE, SW, SE). Default is true.
    /// </summary>
    public bool AllowDiagonalMovement { get; set; } = true;

    /// <summary>
    /// Prevents diagonal movement through tight corners (e.g., walls at N and E block NE movement). Default is true.
    /// </summary>
    public bool PreventCornerCutting { get; set; } = true;

    /// <summary>
    /// Optional toggle to use heuristic for A* pathfinding. If false, uses Dijkstra-like search.
    /// </summary>
    public bool UseHeuristic { get; set; } = true;

    /// <summary>
    /// Maximum pathfinding cost allowed. Used to limit search range. Default is unlimited.
    /// </summary>
    public float MaxSearchCost { get; set; } = float.MaxValue;

    /// <summary>
    /// Optional predicate to define dynamic walkability at runtime (e.g., "avoid fire" or "don't step on mines").
    /// </summary>
    public Func<Vector2Int, bool> AdditionalWalkabilityCheck { get; set; } = null;

    /// <summary>
    /// Optional override for movement cost — useful for terrain penalties, dynamic modifiers, etc.
    /// If null, defaults to IGridMap.GetMovementCost().
    /// </summary>
    public Func<Vector2Int, float> MovementCostOverride { get; set; } = null;
}

public class Pathfinder
{
    private readonly IGridMap map;

    public Pathfinder(IGridMap map)
    {
        this.map = map;
    }


    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, PathfindingSettings settings = null)
    {
        var openSet = new SortedSet<NodeRecord>(new NodeRecordComparer());
        var allNodes = new Dictionary<Vector2Int, NodeRecord>();
        var closedSet = new HashSet<Vector2Int>();


        NodeRecord startRecord = new NodeRecord
        {
            position = start,
            gCost = 0f,
            hCost = Heuristic(start, goal),
            parent = null
        };

        openSet.Add(startRecord);
        allNodes[start] = startRecord;

        while (openSet.Count > 0)
        {
            NodeRecord current = openSet.Min;
            openSet.Remove(current);
            closedSet.Add(current.position);

            if (current.position == goal)
                return RetracePath(current);

            foreach (Vector2Int neighbor in GetNeighbors(current.position, settings))
            {
                if (!map.CheckBoundry(neighbor) || !map.CheckWalkable(neighbor) || closedSet.Contains(neighbor))
                    continue;

                float tentativeG = current.gCost + 1f;
                NodeRecord neighborRecord;

                if (!allNodes.TryGetValue(neighbor, out neighborRecord))
                {
                    neighborRecord = new NodeRecord
                    {
                        position = neighbor,
                        gCost = tentativeG,
                        hCost = Heuristic(neighbor, goal),
                        parent = current
                    };
                    allNodes[neighbor] = neighborRecord;
                    openSet.Add(neighborRecord);
                }
                else if (tentativeG < neighborRecord.gCost)
                {
                    openSet.Remove(neighborRecord); // Must remove before updating, SortedSet needs re-sort
                    neighborRecord.gCost = tentativeG;
                    neighborRecord.parent = current;
                    openSet.Add(neighborRecord);
                }
            }
        }

        return null; // No path found
    }

    private float Heuristic(Vector2Int a, Vector2Int b)
    {
        // Diagonal distance heuristic (Octile)
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * Mathf.Min(dx, dy) + 14 * Mathf.Abs(dx - dy);
    }

    private List<Vector2Int> RetracePath(NodeRecord endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        NodeRecord current = endNode;

        while (current != null)
        {
            path.Add(current.position);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }


    private static readonly Vector2Int[] cardinal = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
    };

    private static readonly Vector2Int[] diagonal = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, -1),
    };

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos, PathfindingSettings settings)
    {
        foreach (var dir in cardinal)
        {
            Vector2Int neighbor = pos + dir;
            if (!map.CheckBoundry(neighbor)) continue;
            if (!map.CheckWalkable(neighbor)) continue;
            if (settings?.AdditionalWalkabilityCheck?.Invoke(neighbor) == false) continue;

            yield return neighbor;
        }

        if (settings?.AllowDiagonalMovement != true)
            yield break;

        foreach (var dir in diagonal)
        {
            Vector2Int neighbor = pos + dir;
            if (!map.CheckBoundry(neighbor)) continue;
            if (!map.CheckWalkable(neighbor)) continue;
            if (settings?.AdditionalWalkabilityCheck?.Invoke(neighbor) == false) continue;

            if (settings?.PreventCornerCutting == true)
            {
                // Diagonal movement requires both adjacent sides to be walkable
                Vector2Int check1 = new Vector2Int(pos.x + dir.x, pos.y);
                Vector2Int check2 = new Vector2Int(pos.x, pos.y + dir.y);

                if (!map.CheckBoundry(check1) || !map.CheckBoundry(check2)) continue;
                if (!map.CheckWalkable(check1) || !map.CheckWalkable(check2)) continue;

                if (settings.AdditionalWalkabilityCheck?.Invoke(check1) == false) continue;
                if (settings.AdditionalWalkabilityCheck?.Invoke(check2) == false) continue;
            }

            yield return neighbor;
        }
    }
}

