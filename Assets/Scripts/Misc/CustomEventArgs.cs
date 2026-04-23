using PlayArk.DialogueSystem.Runtime;
using UnityEngine;
public struct DialogueShowPanelEventArgs : IEventArgs
{
    public E_DialogueExternalUiPanelType UiPanelType;
    public DialogueShowPanelEventArgs(E_DialogueExternalUiPanelType type)
    {
        UiPanelType = type;
    }
}
public struct DialoguePanelFinishedEventArgs : IEventArgs
{
    //面板选项顺序
    public int Index;
    public DialoguePanelFinishedEventArgs(int index)
    {
        Index = index;
    }
}
public struct GameBossDeadEventArgs : IEventArgs
{
    public E_BossType BossType;
    public GameObject BossGameObject;
    public GameBossDeadEventArgs(E_BossType bossType,  GameObject bossGameObject)
    {
        BossType = bossType;
        BossGameObject = bossGameObject;
    }
}

public struct GameBossKeepDeadEventArgs : IEventArgs
{
    public E_BossType BossType;
    public GameBossKeepDeadEventArgs(E_BossType bossType)
    {
        BossType = bossType;
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
    public E_BonusEffectType BonusEffectType;
    public int RestoreAmount;
    public GameBonusEffectEventArgs(E_BonusEffectType bonusEffectType, int restoreAmount)
    {
        BonusEffectType = bonusEffectType;
        RestoreAmount = restoreAmount;
    }
}