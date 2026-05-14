using DreamSenker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSenker.Dialogue.Conditions
{
    /// <summary>
    /// 狐狸Boss已遭遇且未击杀的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "FoxBossAliveCondition_", menuName = "DreamSenker/Dialogue Conditions/Fox Boss Alive")]
    public class FoxBossAliveConditionSO : DialogueConditionSO
    {
        /// <summary>
        /// 判断狐狸Boss是否已遭遇且仍存活。
        /// </summary>
        public override bool IsMet()
        {
            return GameManager.Instance.GameSaveData.IsMetFoxBoss &&
                   !GameManager.Instance.GameSaveData.IsKilledFoxBoss;
        }
    }
}
