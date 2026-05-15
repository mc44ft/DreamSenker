using DreamSeeker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSeeker.Dialogue.Conditions
{
    /// <summary>
    /// 蜘蛛Boss已遭遇且已击杀的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "SpiderBossKilledCondition_", menuName = "ScriptableObject/Dialogue Conditions/Spider Boss Killed")]
    public class SpiderBossKilledConditionSO : DialogueConditionSO
    {
        /// <summary>
        /// 判断蜘蛛Boss是否已遭遇且已击杀。
        /// </summary>
        public override bool IsMet()
        {
            return GameManager.Instance.GameSaveData.IsMetSpiderBoss &&
                   GameManager.Instance.GameSaveData.IsKilledSpiderBoss;
        }
    }
}
