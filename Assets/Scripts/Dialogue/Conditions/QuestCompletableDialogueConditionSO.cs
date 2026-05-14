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
    /// 要判断的任务配置。
    /// </summary>
    [SerializeField] private QuestDefinitionSO _questDefinition;
    /// <summary>
    /// 期望的可交付结果。
    /// </summary>
    [SerializeField] private bool _expectedCanComplete = true;

    /// <summary>
    /// 判断当前任务可交付结果是否等于期望结果。
    /// </summary>
    public override bool IsMet()
    {
        if (_questDefinition == null || string.IsNullOrWhiteSpace(_questDefinition.QuestId))
        {
            Debug.LogError("任务可交付对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }

        return QuestManager.Instance.CanCompleteQuest(_questDefinition.QuestId) == _expectedCanComplete;
    }
}
}
