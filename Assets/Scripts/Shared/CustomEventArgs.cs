using PlayArk.DialogueSystem.Runtime;
using UnityEngine;

namespace DreamSenker.Shared
{
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
