using System.Collections;
using UnityEngine;
using static TurnManager;

public enum Team
{
    Player,
    Enemy
}

public class Character : MonoBehaviour
{
    public string Name = "NaN";
    public float MaxMoveSpeed = 5f;
    public int maxHP = 100;
    public int HP;
    public int AC = 14;
    public int atkRange = 5;
    private int atkMod = 4;
    public int DMG = 8;
    public int DMGMod = 2; // Damage modifier
    public int InitiativeMod = 2;
    public int Initiative;
    public int damageTaken;
    public Team team;
    private HealthBar healthBar;
    public static event System.Action<Character> OnCharacterDeath;

    void OnEnable() => UnitRegistry.Register(this);
    void OnDisable() => UnitRegistry.Deregister(this);

    public void Awake()
    {
        healthBar = GetComponentInChildren<HealthBar>();
        HP = maxHP; // Initialize HP to max at start
        healthBar.SetName(Name);
        healthBar.UpdateHealthBar(HP, maxHP);
    }
    public bool IsAlive()
    {
        if (HP <= 0)
        {
            Debug.Log($"{Name} is dead");
            return false;
        }
        return true;
    }

    public int RollToHit()
    {
        int roll = Random.Range(1, 21); // Simulate a d20 roll
        int total = roll + atkMod; // Add attack modifier
        Debug.Log($"{Name} rolled {roll} + {atkMod} = {total} to hit");
        return total;
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        if (HP <= 0)
        {
            Die();
        }
        else
        {
            healthBar.UpdateHealthBar(HP, maxHP);
        }
    }

    private void Die()
    {
        // Notify all systems BEFORE destroying
        OnCharacterDeath?.Invoke(this);

        // Play death animation/effects before destroying
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {

        var combatLog = FindAnyObjectByType<CombatLog>();

        combatLog.LogUnitDeath(Name);

        yield return new WaitForSeconds(1f);

        Destroy(gameObject);
    }

}
