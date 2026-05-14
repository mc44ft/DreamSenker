using DreamSenker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSenker.Dialogue.Conditions
{
    /// <summary>
    /// 已找到晨露梦核的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "FoundChenCondition_", menuName = "ScriptableObject/Dialogue Conditions/Found Chen")]
    public class FoundChenConditionSO : DialogueConditionSO
    {
        /// <summary>
        /// 判断玩家是否已经获得晨露梦核。
        /// </summary>
        public override bool IsMet()
        {
            return GameManager.Instance.GameSaveData.IsGotChen;
        }
    }
}
