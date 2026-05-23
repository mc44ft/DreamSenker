using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.Conditions
{
/// <summary>
/// 按任务长期状态判断对话分支是否满足。
/// </summary>
[CreateAssetMenu(fileName = "QuestStateCondition_", menuName = "ScriptableObject/Dialogue Conditions/Quest State")]
public class QuestStateConditionSO : ConditionSO
{
    /// <summary>
    /// 期望的任务长期状态。
    /// </summary>
    [SerializeField] private EQuestState _expectedState;
    

    /// <summary>
    /// 判断传入任务的长期状态是否等于期望状态。
    /// </summary>
    public override bool IsMet(ConditionContext context)
    {
        if (context == null)
        {
            Debug.LogError("Quest context is null");
            return false;
        }

        QuestDefinitionSO questDefinition = context.QuestDefinition;
        if (questDefinition == null || string.IsNullOrWhiteSpace(questDefinition.QuestId))
        {
            Debug.LogError("任务状态对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }

        return QuestManager.Instance.GetQuestState(questDefinition.QuestId) == _expectedState;
    }
}
}
