using UnityEngine;

namespace DreamSeeker.Shared
{
public struct GameBossDeadEventArgs : IEventArgs
{
    public EBossType BossType;
    public GameObject BossGameObject;
    public GameBossDeadEventArgs(EBossType BossType,  GameObject bossGameObject)
    {
        this.BossType = BossType;
        BossGameObject = bossGameObject;
    }
}

public struct GameBossKeepDeadEventArgs : IEventArgs
{
    public EBossType BossType;
    public GameBossKeepDeadEventArgs(EBossType BossType)
    {
        this.BossType = BossType;
    }
}
public struct PlayerHealthUpdateEventArgs : IEventArgs
{
    public int MaxHealthAmount;
    public int CurrentHealthAmount;
    public PlayerHealthUpdateEventArgs(int maxHealthAmount, int currentHealthAmount)
    {
        MaxHealthAmount = maxHealthAmount;
        CurrentHealthAmount = currentHealthAmount;
    }
}
public struct GameBonusEffectEventArgs : IEventArgs
{
    public EBonusEffectType BonusEffectType;
    public int RestoreAmount;
    public GameBonusEffectEventArgs(EBonusEffectType BonusEffectType, int restoreAmount)
    {
        this.BonusEffectType = BonusEffectType;
        RestoreAmount = restoreAmount;
    }
}
}
