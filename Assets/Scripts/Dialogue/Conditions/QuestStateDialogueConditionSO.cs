using PlayArk.DialogueSystem.Data;
using UnityEngine;

using DreamSenker.QuestSystem;
using DreamSenker.QuestSystem.Data;

namespace DreamSenker.Dialogue.Conditions
{
/// <summary>
/// 按任务长期状态判断对话分支是否满足。
/// </summary>
[CreateAssetMenu(fileName = "QuestStateCondition_", menuName = "ScriptableObject/Dialogue Conditions/Quest State")]
public class QuestStateDialogueConditionSO : DialogueConditionSO
{
    /// <summary>
    /// 要判断的任务配置。
    /// </summary>
    [SerializeField] private QuestDefinitionSO _questDefinition;
    /// <summary>
    /// 期望的任务长期状态。
    /// </summary>
    [SerializeField] private EQuestState _expectedState;

    /// <summary>
    /// 判断当前任务状态是否等于期望状态。
    /// </summary>
    public override bool IsMet()
    {
        if (_questDefinition == null || string.IsNullOrWhiteSpace(_questDefinition.QuestId))
        {
            Debug.LogError("任务状态对话条件配置错误：QuestDefinitionSO 或 QuestId 为空");
            return false;
        }

        return QuestManager.Instance.GetQuestState(_questDefinition.QuestId) == _expectedState;
    }
}
}
