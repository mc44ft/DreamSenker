using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSenker.QuestSystem;
using DreamSenker.QuestSystem.Data;

namespace DreamSenker.Dialogue.Conditions
{
/// <summary>
/// 按任务当前是否可交付判断对话分支是否满足。
/// </summary>
[CreateAssetMenu(fileName = "QuestCompletableCondition_", menuName = "ScriptableObject/Dialogue Conditions/Quest Completable")]
public class QuestCompletableDialogueConditionSO : DialogueConditionSO
{
    /// <summary>
    /// 期望的可交付结果。
    /// </summary>
    [SerializeField] private bool _expectedCanComplete = true;

    /// <summary>
    /// 任务可交付条件必须由 NPC 对话配置传入任务上下文。
    /// </summary>
    public override bool IsMet()
    {
        Debug.LogError("任务可交付对话条件缺少任务上下文：请在 DialogueNpcTriggerInfo 上配置 QuestDefinition");
        return false;
    }

    /// <summary>
    /// 判断传入任务当前是否可交付，并与期望结果比较。
    /// </summary>
    public override bool IsMet(QuestDefinitionSO questDefinition)
    {
        if (questDefinition == null || string.IsNullOrWhiteSpace(questDefinition.QuestId))
        {
            Debug.LogError("任务可交付对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }

        return QuestManager.Instance.CanCompleteQuest(questDefinition.QuestId) == _expectedCanComplete;
    }
}
}
