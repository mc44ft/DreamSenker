using DreamSeeker.Shared;
using UnityEngine;

namespace DreamSeeker.Characters.Bosses
{
/// <summary>
/// Boss 死亡事件，携带 Boss 类型和对应场景对象。
/// </summary>
public readonly struct GameBossDiedEvent
{
    /// <summary>
    /// 已死亡 Boss 的类型。
    /// </summary>
    public EBossType BossType { get; }

    /// <summary>
    /// 已死亡 Boss 对应的场景对象。
    /// </summary>
    public GameObject BossGameObject { get; }

    /// <summary>
    /// 创建 Boss 死亡事件。
    /// </summary>
    public GameBossDiedEvent(EBossType bossType, GameObject bossGameObject)
    {
        BossType = bossType;
        BossGameObject = bossGameObject;
    }
}

/// <summary>
/// 重新进入地图时保持指定 Boss 死亡状态的事件。
/// </summary>
public readonly struct GameBossKeepDeadEvent
{
    /// <summary>
    /// 需要保持死亡状态的 Boss 类型。
    /// </summary>
    public EBossType BossType { get; }

    /// <summary>
    /// 创建 Boss 永久死亡状态事件。
    /// </summary>
    public GameBossKeepDeadEvent(EBossType bossType)
    {
        BossType = bossType;
    }
}
}
