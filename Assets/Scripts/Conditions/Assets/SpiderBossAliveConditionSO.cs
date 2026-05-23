using DreamSeeker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSeeker.Conditions
{
    /// <summary>
    /// 蜘蛛Boss已遭遇且未击杀的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "SpiderBossAliveCondition_", menuName = "ScriptableObject/Dialogue Conditions/Spider Boss Alive")]
    public class SpiderBossAliveConditionSO : ConditionSO
    {
        /// <summary>
        /// 判断蜘蛛Boss是否已遭遇且仍存活。
        /// </summary>
        public override bool IsMet(ConditionContext context)
        {
            return GameManager.Instance.GameSaveData.IsMetSpiderBoss &&
                   !GameManager.Instance.GameSaveData.IsKilledSpiderBoss;
        }
    }
}
