using System;
using UnityEngine;

using DreamSeeker.Shared;

namespace DreamSeeker.QuestSystem.Data
{
/// <summary>
/// 单个任务交付物品需求。
/// </summary>
[Serializable]
public class QuestItemRequirement
{
    /// <summary>
    /// 需要交付的物品类型。
    /// </summary>
    public EPackageItemType ItemType;
    /// <summary>
    /// 需要交付的物品数量。
    /// </summary>
    [Min(1)] public int Amount = 1;
}
}
