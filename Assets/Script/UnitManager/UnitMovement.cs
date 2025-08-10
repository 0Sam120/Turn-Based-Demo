using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1000f; // Movement speed of the unit
    GridObject m_GridObject; // Reference to this unit's grid object
    List<Vector3> pathWorldPosition; // List of world positions to move through
    AnimationControlller m_AnimationControlller; // Reference to the animation controller
    public bool isMoving = false; // Is the unit currently moving?

    private void Awake()
    {
        m_GridObject = GetComponent<GridObject>();
        m_AnimationControlller = GetComponentInChildren<AnimationControlller>();
    }

    // Initiates movement along a given path
    internal bool Move(List<Vector2Int> path)
    {
        pathWorldPosition = m_GridObject.targetGrid.ConvertPathToWorldPosition(path);

        m_GridObject.targetGrid.RemoveObject(m_GridObject.positionOnGrid, m_GridObject);

        // Update the unit's grid position to the final node of the path
        m_GridObject.positionOnGrid.x = path[path.Count - 1].x;
        m_GridObject.positionOnGrid.y = path[path.Count - 1].y;

        m_GridObject.targetGrid.PlaceObject(m_GridObject.positionOnGrid, m_GridObject);

        RotateTowards(); // Face the first target
        m_AnimationControlller.StartMoving(); // Trigger move animation
        isMoving = true;

        return true;
    }

    private void Update()
    {
        if (pathWorldPosition == null) { return; }
        if (pathWorldPosition.Count == 0) { return; }

        // Move towards the first point in the path
        transform.position = Vector3.MoveTowards(transform.position, pathWorldPosition[0], moveSpeed * Time.deltaTime);

        // When close enough to the current point, remove it and target the next
        if (Vector3.Distance(transform.position, pathWorldPosition[0]) < 0.05f)
        {
            pathWorldPosition.RemoveAt(0);

            if (pathWorldPosition.Count == 0)
            {
                m_AnimationControlller.StopMoving(); // Stop move animation
                isMoving = false; // Unit has stopped
            }
            else
            {
                RotateTowards(); // Rotate towards the next point
            }
        }
    }

    // External access to movement status
    public bool IsMoving()
    {
        return isMoving;
    }

    public bool PathIsValid(List<Vector2Int> path)
    {
        if (isMoving) return false;

        var pathWorldPosition = m_GridObject.targetGrid.ConvertPathToWorldPosition(path);

        return pathWorldPosition.Count != 0;
    }

    // Rotates the unit to face the next movement target
    private void RotateTowards()
    {
        Vector3 direction = (pathWorldPosition[0] - transform.position).normalized;
        direction.y = 0; // Ignore vertical difference
        transform.rotation = Quaternion.LookRotation(direction);
    }
}

