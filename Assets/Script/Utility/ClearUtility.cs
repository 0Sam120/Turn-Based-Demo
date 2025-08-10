using UnityEngine;

public class ClearUtility : MonoBehaviour
{

    // Reference to the grid renderer used for attack highlights
    [SerializeField] GridRenderer attackHighlight;

    // Reference to the grid renderer used for movement highlights
    [SerializeField] GridRenderer moveHighlight;


    // Hides the grid highlight for attack range
    public void ClearGridHighlightAttack()
    {
        attackHighlight.Hide();
    }

    // Hides the grid highlight for movement range
    public void ClearGridHighlightMove()
    {
        moveHighlight.Hide();
    }

    // Fully clears all pathfinding and grid highlights (attack and move)
    public void FullClear()
    {
        ClearGridHighlightMove();
        ClearGridHighlightAttack();
    }
}
