using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    GridMap grid; // Reference to the GridMap component
    [SerializeField] GameObject highlightPoint; // Prefab for cell visualization
    [SerializeField] GameObject container;
    List<GameObject> highlightPointGO; // List to hold spawned move point GameObjects


    private void Awake()
    {
        grid = GetComponentInParent<GridMap>(); // Get the GridMap component attached to the same GameObject
        highlightPointGO = new List<GameObject>(); // Initialize the list of move points
    }

    private GameObject CreatePointHighlightObject()
    {
        GameObject go = Instantiate(highlightPoint);
        highlightPointGO.Add(go);
        go.transform.SetParent(container.transform);
        return go;
    }

    public void fieldHighlight(List<Vector2Int> position)
    {
        for (int i = 0; i < position.Count; i++)
        {
            Highlight(position[i].x, position[i].y, GetPointGO(i));
        }
    }

    public void fieldHighlight(List<PathNode> position)
    {
        for (int i = 0; i < position.Count; i++)
        {
            Highlight(position[i].x, position[i].y, GetPointGO(i));
        }
    }

    internal void Hide()
    {
        for(int i = 0; i < highlightPointGO.Count; i++)
        {
            highlightPointGO[i].SetActive(false);
        }
    }

    private GameObject GetPointGO(int i)
    {
        if(highlightPointGO.Count > i)
        {
            return highlightPointGO[i];
        }

        GameObject newHighlightObject = CreatePointHighlightObject();
        return newHighlightObject;
    }

    private void Highlight(int posX, int posY, GameObject highlightObject)
    {
        highlightObject.SetActive(true);
        Vector3 position = grid.GetWorldPosition(posX, posY, true);
        position += Vector3.up * 0.2f;
        highlightObject.transform.position = position;
    }
}
