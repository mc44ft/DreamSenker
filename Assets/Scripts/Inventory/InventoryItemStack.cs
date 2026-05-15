using DreamSeeker.Data.Configs;
using DreamSeeker.Shared;

namespace DreamSeeker.Inventory
{
/// <summary>
/// 背包单个格子的展示数据
/// </summary>
public readonly struct InventoryItemStack
{
    /// <summary>
    /// 物品类型标识
    /// </summary>
    public EPackageItemType ItemType { get; }
    /// <summary>
    /// 当前物品数量
    /// </summary>
    public int Count { get; }
    /// <summary>
    /// 当前物品配置
    /// </summary>
    public PackageItemInfo ItemInfo { get; }

    /// <summary>
    /// 创建背包格子展示数据
    /// </summary>
    public InventoryItemStack(EPackageItemType itemType, int count, PackageItemInfo itemInfo)
    {
        ItemType = itemType;
        Count = count;
        ItemInfo = itemInfo;
    }
}
}
