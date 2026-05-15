using DreamSeeker.Data.Configs;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.Inventory
{
/// <summary>
/// 统一处理可使用道具的效果执行
/// </summary>
public class PropEffectResolver
{
    /// <summary>
    /// 根据物品配置执行对应效果
    /// </summary>
    public bool TryApply(PackageItemInfo itemInfo)
    {
        if (itemInfo == null || itemInfo.BonusEffectType == EBonusEffectType.None)
        {
            return false;
        }

        switch (itemInfo.BonusEffectType)
        {
            case EBonusEffectType.RestoreHealth:
                GameManager.Instance.Player.DamageableHealth.RestoreHealth(itemInfo.BonusAmount);
                break;
        }

        return true;
    }
}
}
