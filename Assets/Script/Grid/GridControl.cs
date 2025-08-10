using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


/// Handles player interaction with the grid-based map.
/// Calculates pathfinding when clicking on the terrain.
/// Draws the current path using Gizmos.
public class GridControl : MonoBehaviour
{
    [SerializeField] GridMap grid;         // Reference to the grid map
    [SerializeField] LayerMask terrain;    // LayerMask to define valid terrain for clicks
    [SerializeField] GridObject hoverOver;
    [SerializeField] SelectableGridObject selected;
    
    private Camera cam;                      // Camera used to raycast from mouse
    private MouseInput mouseActions;

    private void Awake()
    {
        mouseActions = new MouseInput();
        // Cache the camera component (assumed to be on the same GameObject)
        cam = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        mouseActions.UnitControl.Enable();
        mouseActions.UnitControl.Select.performed += Select;
        mouseActions.UnitControl.Deselect.performed += Deselect;
    }

    private void OnDisable()
    {
        mouseActions.UnitControl.Disable();
        mouseActions.UnitControl.Select.performed -= Select;
        mouseActions.UnitControl.Deselect.performed -= Deselect;
    }

    private void Update()
    {
        HoverOverObjectCheck();
    }

    private void HoverOverObjectCheck()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the ray hits the terrain layer
        if (Physics.Raycast(ray, out hit, float.MaxValue, terrain))
        {
            // Convert hit position to grid coordinates
            Vector2Int gridPosition = grid.GetGridPosition(hit.point);
            GridObject gridObject = grid.GetPlacedObject(gridPosition);
            hoverOver = gridObject;
        }
    }

    private void Select(InputAction.CallbackContext context)
    {
        if (hoverOver == null) { return; }
        selected = hoverOver.GetComponent<SelectableGridObject>();
    }
    private void Deselect(InputAction.CallbackContext context)
    {
        selected = null;
    }
}
