using UnityEngine;
using UnityEngine.EventSystems;

public class CursorData : MonoBehaviour
{
    // Reference to the main camera
    [SerializeField] Camera mainCamera;

    // Reference to the grid map
    [SerializeField] GridMap targetGrid;

    // Layer mask for terrain detection
    [SerializeField] LayerMask terrainMask;

    // Current position of the cursor on the grid
    public Vector2Int positionOnGrid;

    private void Update()
    {
        // Create a ray from the mouse position into the scene
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Perform raycast against the terrain layer
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrainMask))
        {
            // Convert world position to grid position
            Vector2Int hitPosition = targetGrid.GetGridPosition(hit.point);

            // Update position if it has changed
            if (hitPosition != positionOnGrid)
            {
                positionOnGrid = hitPosition;
            }
        }
    }
}

