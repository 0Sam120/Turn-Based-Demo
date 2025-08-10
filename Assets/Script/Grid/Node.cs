using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CoverType
{
    None,
    Half,
    Full
}

// Represents a single cell/node in the grid
public class Node
{
    public bool passable; // Whether this node can be walked on or not
    public GridObject gridObject; // Reference to any object placed on this node
    public float elevation; // Height (Y-axis value) of the terrain at this node

    public NodeEdge[] edges = new NodeEdge[4]; // N, E, S, W

}

public class NodeEdge
{
    public CoverType coverType;
    public bool blocksMovement;
    public bool blocksLineOfSight;

    // Optional: link back to adjacent nodes if needed
    public Node from;
    public Node to;
}
