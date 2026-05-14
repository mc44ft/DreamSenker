using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSenker.Dialogue.Conditions
{
    /// <summary>
    /// 永远满足的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "AlwaysTrueCondition_", menuName = "ScriptableObject/Dialogue Conditions/Always True")]
    public class AlwaysTrueConditionSO : DialogueConditionSO
    {
        /// <summary>
        /// 始终返回满足。
        /// </summary>
        public override bool IsMet()
        {
            return true;
        }
    }
}
