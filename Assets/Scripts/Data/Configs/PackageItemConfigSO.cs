using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

using DreamSenker.Shared;

namespace DreamSenker.Data.Configs
{
[CreateAssetMenu(fileName = "PackageItemConfig_", menuName = "ScriptableObject/Config/PackageItemConfig")]
public class PackageItemConfigSO : ScriptableObject
{
    [field: SerializeField] public PackageItemInfo[] PackageItemInfoArray;
    public PackageItemInfo GetItemInfoByID(EPackageItemType type)
    {
        return PackageItemInfoArray.Where(info => info.ePackageItemType == type).FirstOrDefault();
    }
}
[Serializable]
public class PackageItemInfo
{
    [FormerlySerializedAs("PackageItemID")] public EPackageItemType ePackageItemType;
    public Sprite Icon;
    [Tooltip("物品描述")]
    [Multiline]
    public string Description;
    [FormerlySerializedAs("BonusEffectType")] [Tooltip("物品加成效果")]
    public EBonusEffectType BonusEffectType;
    [Tooltip("物品加成数值")]
    public int BonusAmount;
}
}
