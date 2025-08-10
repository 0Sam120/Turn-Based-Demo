using UnityEngine;

public class CoverProp : MonoBehaviour
{
    public Vector2Int nodeA;
    public Vector2Int nodeB;
    public CoverType coverType = CoverType.Full;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = coverType == CoverType.Full ? Color.red : Color.yellow;
        Vector3 a = new Vector3(nodeA.x + 0.5f, 0, nodeA.y + 0.5f);
        Vector3 b = new Vector3(nodeB.x + 0.5f, 0, nodeB.y + 0.5f);
        Gizmos.DrawLine(a, b);
    }
#endif
}
