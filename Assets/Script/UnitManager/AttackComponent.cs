using UnityEngine;

public class AttackComponent : MonoBehaviour
{
    // Reference to this object's grid data
    GridObject gridObject;

    // Reference to the character's animation controller
    AnimationControlller characterAnimator;
    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
        // Get the GridObject component attached to this GameObject
        gridObject = GetComponent<GridObject>();

        // Get the animation controller from child objects
        characterAnimator = GetComponentInChildren<AnimationControlller>();
    }

    // Called when this character attacks a target on the grid
    public void AttackPosition(GridObject targetGridObject, int total)
    {
        Debug.Log("Target Found");

        var combatLog = FindAnyObjectByType<CombatLog>();

        // Rotate this character to face the target
        RotateCharacter(targetGridObject.transform.position);
        if(total >= targetGridObject.GetComponent<Character>().AC)
        {
            // If the attack hits, deal damage to the target character
            int damage = Random.Range(1, character.DMG) + character.DMGMod; // Get the damage value from this character
            targetGridObject.GetComponent<Character>().TakeDamage(damage);
            combatLog.LogAttack(character.Name, targetGridObject.GetComponent<Character>().Name, total, damage);
        }
        else
        {
            // If the attack misses, log a message
            combatLog.LogMiss(character.Name, targetGridObject.GetComponent<Character>().Name, total);
        }

        // Play the attack animation
        characterAnimator.Attack();
    }

    // Rotates the character to face towards a world position
    private void RotateCharacter(Vector3 towards)
    {
        // Calculate direction to the target
        Vector3 direction = (towards - transform.position).normalized;

        // Prevent rotation on the Y axis (keep character upright)
        direction.y = 0;

        // Apply the rotation to face the target
        transform.rotation = Quaternion.LookRotation(direction);
    }
}

