using System;
using UnityEngine;

namespace DreamSeeker.Combat.Health
{
public class Health : MonoBehaviour
{
    public int MaxHealthAmount { get; private set; }
    public int CurrentHealthAmount { get; private set; }

    public bool IsDead { get; private set; } = false;
    public event Action<int, int> OnHealthChanged;
    public event Action OnDead;

    public void Initialize(int maxHealthAmount, int currentHealthAmount)
    {
        IsDead = false;

        MaxHealthAmount = maxHealthAmount;
        CurrentHealthAmount = currentHealthAmount;
        NotifyHealthChanged();
    }
    public void ApplyDamage(int damage)
    {
        CurrentHealthAmount -= damage;
        NotifyHealthChanged();
        
        if (CurrentHealthAmount <= 0 )
        {
            Death();
        }
    }
    public void RestoreHealth(int healthAmount)
    {
        CurrentHealthAmount = Mathf.Min(CurrentHealthAmount + healthAmount, MaxHealthAmount);
        NotifyHealthChanged();
    }
    private void Death()
    {
        if (IsDead) return;

        IsDead = true;
        OnDead?.Invoke();
    }

    private void NotifyHealthChanged()
    {
        // 向外通知血量变化，具体表现仍由各自 Controller 处理。
        OnHealthChanged?.Invoke(MaxHealthAmount, CurrentHealthAmount);
    }
    
}
}
