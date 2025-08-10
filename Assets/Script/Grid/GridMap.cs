using System;
using System.Collections.Generic;
using UnityEngine;

public class GridMap : MonoBehaviour, IGridMap
{
    Node[,] grid; // 2D array to hold all grid nodes
    public int width = 25; // Number of cells along the width
    public int length = 25; // Number of cells along the length
    [SerializeField] float cellSize = 1f; // Size of each cell
    [SerializeField] LayerMask obstacle; // Layer used to detect obstacles
    [SerializeField] LayerMask terrain; // Layer used to detect terrain
    [SerializeField] private LayerMask coverLayer; // Layer for cover props (walls, fences, etc.)
    [SerializeField] private GameObject shieldSpritePrefab; // Visual marker

    private void Awake()
    {
        GenerateGrid(); // Create the grid when the scene starts
        PopulateCover(); // Populate cover information for each cell
    }

    private void GenerateGrid()
    {
        grid = new Node[length, width]; // Initialize the grid array

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                Node node = new Node();
                grid[x, y] = node; // Create a new Node at each cell

                if(y > 0)
                {
                    Node northNeighbour = grid[x, y - 1];
                    NodeEdge edge = new NodeEdge
                    {
                        from = node,
                        to = northNeighbour,
                        coverType = CoverType.None,
                        blocksMovement = false,
                        blocksLineOfSight = false
                    };

                    node.edges[0] = edge; // North edge
                    northNeighbour.edges[2] = edge; // South edge
                }

                if(x > 0)
                {
                    Node westNeighbour = grid[x - 1, y];
                    NodeEdge edge = new NodeEdge
                    {
                        from = node,
                        to = westNeighbour,
                        coverType = CoverType.None,
                        blocksMovement = false,
                        blocksLineOfSight = false
                    };

                    node.edges[3] = edge; // West edge
                    westNeighbour.edges[1] = edge; // East edge
                }
            }
        }

        CalculateElevation(); // Assign elevation to each cell
        CheckPassableGrid(); // Check for obstacles in each cell
    }

    private void CalculateElevation()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                // Raycast downward to find terrain surface
                Ray ray = new Ray(GetWorldPosition(x, y) + Vector3.up * 100f, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue, terrain))
                {
                    grid[x, y].elevation = hit.point.y; // Set node's elevation
                }
            }
        }
    }

    public bool CheckBoundry(Vector2Int positionOnGrid)
    {
        // Check if the position is within the grid bounds
        if (positionOnGrid.x < 0 || positionOnGrid.x >= length)
        {
            return false;
        }
        if (positionOnGrid.y < 0 || positionOnGrid.y >= width)
        {
            return false;
        }

        return true;
    }

    private void CheckPassableGrid()
    {
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < length; x++)
            {
                // Check if there's an obstacle at the node's world position
                Vector3 worldPosition = GetWorldPosition(x, y);
                bool passable = !Physics.CheckBox(worldPosition, Vector3.one / 2 * cellSize, Quaternion.identity, obstacle);
                grid[x, y].passable = passable; // Mark node as passable or not
            }
        }
    }

    private void PopulateCover()
    {
        var props = FindObjectsByType<CoverProp>(FindObjectsSortMode.None);

        foreach(var prop in props)
        {
            if(CheckBoundry(prop.nodeA) == false || CheckBoundry(prop.nodeB) == false)
            {
                Debug.LogWarning($"CoverProp {prop.name} is out of bounds: A({prop.nodeA}) B({prop.nodeB})");
                continue;
            }

            Node a = grid[prop.nodeA.x, prop.nodeA.y];
            Node b = grid[prop.nodeB.x, prop.nodeB.y];

            Vector2Int delta = prop.nodeB - prop.nodeA;
            int dirFromA = DirectionToIndex(delta);
            int dirFromB = (dirFromA + 2) % 4; // Opposite direction

            NodeEdge edge = a.edges[dirFromA];
            if (edge == null) continue;

            edge.coverType = prop.coverType; // Set cover type
            edge.blocksLineOfSight = true;

            Vector3 posA = GetWorldPosition(prop.nodeA.x, prop.nodeA.y);
            Vector3 posB = GetWorldPosition(prop.nodeB.x, prop.nodeB.y);
            Vector3 centre = (posA + posB) / 2;
            Vector3 forward = (posB - posA).normalized;

            Instantiate(shieldSpritePrefab, centre + Vector3.up * 1.5f, Quaternion.LookRotation(forward));
        }

    }


    int DirectionToIndex(Vector2Int offset)
    {
        if (offset == Vector2Int.up) return 0;    // North
        if (offset == Vector2Int.right) return 1; // East
        if (offset == Vector2Int.down) return 2;  // South
        if (offset == Vector2Int.left) return 3;  // West
        Debug.LogWarning("Invalid offset: " + offset);
        return -1;
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Convert a world position into a grid coordinate
        worldPosition.x += cellSize / 2;
        worldPosition.y += cellSize / 2;
        Vector2Int positionOfGrid = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
        return positionOfGrid;
    }

    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            // If no grid exists yet, draw basic placeholders
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = GetWorldPosition(x, y);
                    Gizmos.DrawCube(pos, Vector3.one / 4);
                }
            }
        }
        else
        {
            // Draw cells with color depending on passability
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < length; x++)
                {
                    Vector3 pos = GetWorldPosition(x, y, true);
                    Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                    Gizmos.DrawCube(pos, Vector3.one / 4);
                }
            }
        }
    }

    public Vector3 GetWorldPosition(int x, int y, bool elevation = false)
    {
        // Get the world position for a grid coordinate
        return new Vector3(x * cellSize, elevation ? grid[x, y].elevation : 0f, y * cellSize);
    }

    internal void RemoveObject(Vector2Int positionOnGrid, GridObject gridObject)
    {
        if (CheckBoundry(positionOnGrid))
        {
            if (grid[positionOnGrid.x, positionOnGrid.y].gridObject != gridObject) { return; }
            grid[positionOnGrid.x, positionOnGrid.y].gridObject = null;
        }
        else
        {
            Debug.Log("Object outside bounds");
        }
    }

    public void PlaceObject(Vector2Int positionOnGrid, GridObject gridObject)
    {
        // Place a GridObject at a specific cell
        if (CheckBoundry(positionOnGrid))
        {
            grid[positionOnGrid.x, positionOnGrid.y].gridObject = gridObject;
        }
        else
        {
            Debug.Log("Character object out of bounds");
        }
    }

    internal GridObject GetPlacedObject(Vector2Int gridPosition)
    {
        // Get the GridObject at a given position
        if (CheckBoundry(gridPosition))
        {
            GridObject gridObject = grid[gridPosition.x, gridPosition.y].gridObject;
            return gridObject;
        }
        return null;
    }

    internal bool CheckBoundry(int posX, int posY)
    {
        // Alternative overload to check bounds with ints instead of Vector2Int
        if (posX < 0 || posX >= length)
        {
            return false;
        }
        if (posY < 0 || posY >= width)
        {
            return false;
        }

        return true;
    }

    public bool CheckWalkable(Vector2Int pos)
    {
        // Return whether a cell is walkable
        return grid[pos.x, pos.y].passable;
    }

    public List<Vector3> ConvertPathToWorldPosition(List<Vector2Int> path)
    {
        List<Vector3> worldPositions = new List<Vector3>();

        if (path == null)
            return worldPositions;

        foreach (var tilePos in path)
        {
            worldPositions.Add(GetWorldPosition(tilePos.x, tilePos.y, true));
        }

        return worldPositions;
    }

}
