using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

using DreamSenker.Shared;

namespace DreamSenker.Data.Configs
{
[CreateAssetMenu(fileName = "PackageItemConfig_", menuName = "ScriptableObject/Config/PackageItemConfig")]
public class PackageItemConfigSO : ScriptableObject
{
    /// <summary>
    /// 所有背包物品类型配置
    /// </summary>
    [field: SerializeField] public PackageItemInfo[] PackageItemInfoArray;

    /// <summary>
    /// 按物品类型查询配置
    /// </summary>
    public PackageItemInfo GetItemInfoByID(EPackageItemType type)
    {
        if (PackageItemInfoArray == null)
        {
            return null;
        }

        return PackageItemInfoArray.Where(info => info.ePackageItemType == type).FirstOrDefault();
    }
}
[Serializable]
public class PackageItemInfo
{
    /// <summary>
    /// 物品类型标识
    /// </summary>
    [FormerlySerializedAs("PackageItemID")] public EPackageItemType ePackageItemType;
    /// <summary>
    /// 物品所属标签页分类
    /// </summary>
    public EPackageItemCategory Category = EPackageItemCategory.Unknown;
    /// <summary>
    /// 物品图标
    /// </summary>
    public Sprite Icon;
    [Tooltip("物品描述")]
    [Multiline]
    public string Description;
    [FormerlySerializedAs("BonusEffectType")] [Tooltip("物品加成效果")]
    public EBonusEffectType BonusEffectType;
    [Tooltip("物品加成数值")]
    public int BonusAmount;
    [Tooltip("物品最大堆叠数量")]
    [Min(1)]
    public int MaxStack = 99;
}
}
