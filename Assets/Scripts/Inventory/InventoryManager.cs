using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DreamSenker.Data.Configs;
using DreamSenker.Data.Runtime;
using DreamSenker.Shared;

namespace DreamSenker.Inventory
{
public class InventoryManager : BaseManager<InventoryManager>
{
    /// <summary>
    /// 当前玩家背包存档数据
    /// </summary>
    public PackageData PackageData {  get; private set; }
    /// <summary>
    /// 背包物品配置表
    /// </summary>
    private PackageItemConfigSO _packageItemConfig;
    /// <summary>
    /// 道具效果执行器
    /// </summary>
    private readonly PropEffectResolver _propEffectResolver = new PropEffectResolver();

    private InventoryManager() { }

    /// <summary>
    /// 注入背包运行时数据和物品配置
    /// </summary>
    public void SetupData(PackageData packageData, PackageItemConfigSO m_packageItemConfig)
    {
        PackageData = packageData;
        this._packageItemConfig = m_packageItemConfig;
    }

    /// <summary>
    /// 清空背包管理器持有的运行时引用
    /// </summary>
    public void ResetData()
    {
        PackageData = null;
        _packageItemConfig = null;
    }

    /// <summary>
    /// 兼容旧调用：向背包添加一个物品
    /// </summary>
    public void AddItemToPackage(EPackageItemType itemType)
    {
        TryAddItem(itemType);
    }

    /// <summary>
    /// 兼容旧调用：从背包移除一个物品
    /// </summary>
    public void RemoveItemFromPackage(EPackageItemType itemType)
    {
        TryRemoveItem(itemType);
    }

    /// <summary>
    /// 尝试向背包添加指定数量物品
    /// </summary>
    public bool TryAddItem(EPackageItemType itemType, int count = 1)
    {
        if (!IsReady() || count <= 0)
        {
            return false;
        }

        PackageItemInfo itemInfo = GetItemInfoByID(itemType);
        int maxStack = GetSafeMaxStack(itemInfo);
        PackageData.PackageDict.TryGetValue(itemType, out int currentCount);

        if (currentCount >= maxStack)
        {
            return false;
        }

        PackageData.PackageDict[itemType] = Mathf.Min(currentCount + count, maxStack);
        return true;
    }

    /// <summary>
    /// 尝试从背包移除指定数量物品
    /// </summary>
    public bool TryRemoveItem(EPackageItemType itemType, int count = 1)
    {
        if (!IsReady() || count <= 0 || !PackageData.PackageDict.TryGetValue(itemType, out int currentCount))
        {
            return false;
        }

        if (currentCount < count)
        {
            return false;
        }

        int nextCount = currentCount - count;
        if (nextCount <= 0)
        {
            PackageData.PackageDict.Remove(itemType);
        }
        else
        {
            PackageData.PackageDict[itemType] = nextCount;
        }

        return true;
    }

    /// <summary>
    /// 尝试使用指定物品，效果成功后扣除数量
    /// </summary>
    public bool TryUseItem(EPackageItemType itemType)
    {
        if (!CanUseItem(itemType))
        {
            return false;
        }

        PackageItemInfo itemInfo = GetItemInfoByID(itemType);
        if (!_propEffectResolver.TryApply(itemInfo))
        {
            return false;
        }

        return TryRemoveItem(itemType);
    }

    /// <summary>
    /// 判断物品当前是否可以使用
    /// </summary>
    public bool CanUseItem(EPackageItemType itemType)
    {
        PackageItemInfo itemInfo = GetItemInfoByID(itemType);
        return itemInfo != null &&
               itemInfo.Category == EPackageItemCategory.Usable &&
               itemInfo.BonusEffectType != EBonusEffectType.None &&
               HasItem(itemType);
    }

    /// <summary>
    /// 判断背包是否拥有指定数量物品
    /// </summary>
    public bool HasItem(EPackageItemType itemType, int count = 1)
    {
        if (!IsReady() || count <= 0)
        {
            return false;
        }

        return PackageData.PackageDict.TryGetValue(itemType, out int currentCount) && currentCount >= count;
    }

    /// <summary>
    /// 获取指定分类下的格子展示数据
    /// </summary>
    public List<InventoryItemStack> GetItemStacks(EPackageItemCategory category)
    {
        if (!IsReady())
        {
            return new List<InventoryItemStack>();
        }

        return PackageData.PackageDict
            .Where(pair => pair.Value > 0)
            .Select(pair => new InventoryItemStack(pair.Key, pair.Value, GetItemInfoByID(pair.Key)))
            .Where(stack => stack.ItemInfo != null && stack.ItemInfo.Category == category)
            .ToList();
    }

    /// <summary>
    /// 按物品类型查询配置
    /// </summary>
    public PackageItemInfo GetItemInfoByID(EPackageItemType type)
    {
        if (_packageItemConfig == null)
        {
            return null;
        }

        return _packageItemConfig.GetItemInfoByID(type);
    }

    /// <summary>
    /// 判断背包数据和配置是否已经注入
    /// </summary>
    private bool IsReady()
    {
        return PackageData != null && PackageData.PackageDict != null && _packageItemConfig != null;
    }

    /// <summary>
    /// 获取有效最大堆叠数，避免未配置时阻塞拾取
    /// </summary>
    private int GetSafeMaxStack(PackageItemInfo itemInfo)
    {
        return itemInfo != null && itemInfo.MaxStack > 0 ? itemInfo.MaxStack : 99;
    }
}
}
