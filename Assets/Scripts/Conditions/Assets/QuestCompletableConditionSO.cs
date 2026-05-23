using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.Conditions
{
/// <summary>
/// 按任务当前是否可交付判断对话分支是否满足。
/// </summary>
[CreateAssetMenu(fileName = "QuestCompletableCondition_", menuName = "ScriptableObject/Dialogue Conditions/Quest Completable")]
public class QuestCompletableConditionSO : ConditionSO
{
    /// <summary>
    /// 期望的可交付结果。
    /// </summary>
    [SerializeField] private bool _expectedCanComplete = true;

    /// <summary>
    /// 判断传入任务当前是否可交付，并与期望结果比较。
    /// </summary>
    public override bool IsMet(ConditionContext context)
    {
        if (context == null)
        {
            Debug.LogError($"{nameof(context)} is null");
            return false;
        }
        QuestDefinitionSO questDefinition = context.QuestDefinition;
        if (questDefinition == null || string.IsNullOrWhiteSpace(questDefinition.QuestId))
        {
            Debug.LogError("任务可交付对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }
        return QuestManager.Instance.CanCompleteQuest(questDefinition.QuestId) == _expectedCanComplete;
    }
}
}
