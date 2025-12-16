using System;
using UnityEngine;
public class Health : MonoBehaviour
{
    public int MaxHealthAmount { get; private set; }
    public int CurrentHealthAmount { get; private set; }

    public bool IsDead { get; private set; } = false;

    public void Initialize(int maxHealthAmount, int currentHealthAmount)
    {
        IsDead = false;

        MaxHealthAmount = maxHealthAmount;
        CurrentHealthAmount = currentHealthAmount;

        //Debug.Log("当前血量" + CurrentHealthAmount);
    }
    public void ApplyDamage(int damage)
    {
        CurrentHealthAmount -= damage;

        //Debug.Log("当前血量" + CurrentHealthAmount + " " + "当前伤害" + damage);
        if (CurrentHealthAmount <= 0 )
        {
            Death();
        }
    }
    public void RestoreHealth(int healthAmount)
    {
        CurrentHealthAmount = Mathf.Min(CurrentHealthAmount + healthAmount, MaxHealthAmount);
    }
    private void Death()
    {
        IsDead = true;
    }
    
}
