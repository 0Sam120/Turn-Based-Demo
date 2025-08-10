using UnityEngine;

public class CharacterTurn : MonoBehaviour
{
    public TurnManager turnManager;
    public int Momentum; // Momentum for the character, used to track actions
    public int momentumGains;
    public int maxMomentumGained;


    public void GrantTurn()
    {
        Momentum = 4;
        maxMomentumGained = Momentum * 2;
        momentumGains = Momentum;
    }

    public bool CanSpendMomentum(int amount)
    {
        return Momentum >= amount;
    }

    public bool SpendMomentum(int amount)
    {
        if (amount <= Momentum)
        {
            Momentum -= amount;
            if (Momentum <= 0)
            {
                AutomaticTurnEnd();
            }
            return true;
            
        }
        else
        {
            Debug.LogWarning($"Not enough momentum to spend {amount}. Current momentum: {Momentum}");
            return false;
        }
    }

    public void GainMomentum(int amount)
    {
        if(momentumGains <= maxMomentumGained)
        {
            Momentum += amount;
            momentumGains += amount;
        }
        else
        {
            Debug.LogWarning($"Cannot gain {amount} momentum. Max momentum is {maxMomentumGained}. Current momentum: {Momentum}");
        }
    }

    public void AutomaticTurnEnd()
    {
        TurnManager.Instance.EndCurrentUnitTurn();
        Debug.Log($"{gameObject.name} has automatically ended their turn.");
    }
}
