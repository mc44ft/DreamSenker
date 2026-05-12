using DreamSenker.Data.Configs;
using DreamSenker.Shared;

namespace DreamSenker.Inventory
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

        // 暂时沿用项目现有加成事件，后续再把具体回血/回蓝落到明确服务上。
        EventCenter.Instance.EventTrigger(
            E_EventType.Game_BonusEffect,
            this,
            new GameBonusEffectEventArgs(itemInfo.BonusEffectType, itemInfo.BonusAmount));

        return true;
    }
}
}
