using System;
using System.Collections.Generic;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    public GridMap targetGrid; // Reference to the grid this object belongs to
    public Vector2Int positionOnGrid; // Position of this object on the grid

    private void Start()
    {
        Init(); // Initialize the object's grid placement
    }

    private void Init()
    {
        // Get the object's grid position based on its world position
        positionOnGrid = targetGrid.GetGridPosition(transform.position);

        // Place this object into the grid at the calculated position
        targetGrid.PlaceObject(positionOnGrid, this);

        // Move the object exactly to the center of its grid cell (with elevation)
        Vector3 pos = targetGrid.GetWorldPosition(positionOnGrid.x, positionOnGrid.y, true);
        transform.position = pos;
    }
}
