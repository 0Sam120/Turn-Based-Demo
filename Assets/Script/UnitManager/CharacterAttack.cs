using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterAttack : MonoBehaviour
{
    // Reference to the grid map to check boundaries and objects
    [SerializeField] GridMap targetGrid;
    
    // Reference to the grid renderer for highlighting attackable tiles
    [SerializeField] GridRenderer highlight;

    // List of tiles that are valid attack positions
    List<Vector2Int> attackPosition;

    // Input handling
    private MouseInput mouseInput;
    
    // Camera reference (unused in this script but initialized)
    private Camera cam;

    private void Awake()
    {
        mouseInput = new MouseInput();
        cam = GetComponent<Camera>();
    }
    
    // Calculates all grid positions within the character's attack range
    public void CalculateAttackArea(Vector2Int characterPositionOnGrid, int attackRange)
    {
        // Initialize or clear the attackPosition list
        if (attackPosition == null)
        {
            attackPosition = new List<Vector2Int>();
        }
        else
        {
            attackPosition.Clear();
        }

        // Loop through a square area around the character based on attack range
        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                // Skip positions outside of Manhattan distance range
                if (Mathf.Abs(x) + Mathf.Abs(y) > attackRange) { continue; }

                // Skip the character's own tile
                if (x == 0 && y == 0) { continue; }

                // Check if this grid position is within the grid boundaries
                if (targetGrid.CheckBoundry(characterPositionOnGrid.x + x, characterPositionOnGrid.y + y))
                {
                    // Add valid attack position
                    attackPosition.Add(new Vector2Int(characterPositionOnGrid.x + x, characterPositionOnGrid.y + y));
                }
            }
        }

        // Highlight all valid attack tiles
        highlight.fieldHighlight(attackPosition);
    }

    // Returns the grid object located at the given grid position
    internal GridObject GetAttackTarget(Vector2Int positionOnGrid)
    {
        GridObject target = targetGrid.GetPlacedObject(positionOnGrid);
        return target;
    }

    // Checks if the given grid position is within the calculated attack area
    internal bool Check(Vector2Int positionOnGrid)
    {
        return attackPosition.Contains(positionOnGrid);
    }

    // Input-based attack function (disabled, logic handled externally)
    private void Attack(InputAction.CallbackContext context)
    {
        // This function is disabled. Attack handling is managed by CommandInput.

        // Example of how it would work:
        // Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        // RaycastHit hit;
        // if (Physics.Raycast(ray, out hit, float.MaxValue, terrainMask))
        // {
        //     Vector2Int gridPosition = targetGrid.GetGridPosition(hit.point);
        //     Debug.Log("Shot");

        //     if (attackPosition.Contains(gridPosition))
        //     {
        //         GridObject gridObject = targetGrid.GetPlacedObject(gridPosition);
        //         if (gridObject == null) { return; }
        //         selectedCharacter.GetComponent<AttackComponent>().AttackPosition(gridObject);
        //         Debug.Log("Found target");
        //     }
        // }
    }
}
