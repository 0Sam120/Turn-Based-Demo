using UnityEngine;

public class AnimationControlller : MonoBehaviour
{
    private Animator animator;

    // Animation parameter hashes (more efficient than strings)
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int AttackHash = Animator.StringToHash("Shoot");

    bool attack;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void StartMoving()
    {
        animator.SetBool(MoveHash, true);
    }

    public void StopMoving()
    {
        animator.SetBool(MoveHash, false);
    }

    public void Attack()
    {
        animator.SetTrigger(AttackHash);
    }
}
