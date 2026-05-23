using DreamSeeker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSeeker.Conditions
{
    /// <summary>
    /// 梦境地图已通关的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "MirrorMapClearedCondition_", menuName = "ScriptableObject/Dialogue Conditions/Mirror Map Cleared")]
    public class MirrorMapClearedConditionSO : ConditionSO
    {
        /// <summary>
        /// 判断梦境地图是否已经通关。
        /// </summary>
        public override bool IsMet(ConditionContext context)
        {
            return GameManager.Instance.GameSaveData.IsClearMirrorMap;
        }
    }
}
