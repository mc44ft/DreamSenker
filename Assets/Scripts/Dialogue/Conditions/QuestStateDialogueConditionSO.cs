using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.Dialogue.Conditions
{
/// <summary>
/// 按任务长期状态判断对话分支是否满足。
/// </summary>
[CreateAssetMenu(fileName = "QuestStateCondition_", menuName = "ScriptableObject/Dialogue Conditions/Quest State")]
public class QuestStateDialogueConditionSO : DialogueConditionSO
{
    /// <summary>
    /// 期望的任务长期状态。
    /// </summary>
    [SerializeField] private EQuestState _expectedState;

    /// <summary>
    /// 任务状态条件必须由 NPC 对话配置传入任务上下文。
    /// </summary>
    public override bool IsMet()
    {
        Debug.LogError("任务状态对话条件缺少任务上下文：请在 DialogueNpcTriggerInfo 上配置 QuestDefinition");
        return false;
    }

    /// <summary>
    /// 判断传入任务的长期状态是否等于期望状态。
    /// </summary>
    public override bool IsMet(QuestDefinitionSO questDefinition)
    {
        if (questDefinition == null || string.IsNullOrWhiteSpace(questDefinition.QuestId))
        {
            Debug.LogError("任务状态对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }

        return QuestManager.Instance.GetQuestState(questDefinition.QuestId) == _expectedState;
    }
}
}
