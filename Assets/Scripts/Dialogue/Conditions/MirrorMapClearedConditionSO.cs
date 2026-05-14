using DreamSenker.Managers;
using PlayArk.DialogueSystem.Data;
using UnityEngine;

namespace DreamSenker.Dialogue.Conditions
{
    /// <summary>
    /// 梦境地图已通关的对话条件。
    /// </summary>
    [CreateAssetMenu(fileName = "MirrorMapClearedCondition_", menuName = "DreamSenker/Dialogue Conditions/Mirror Map Cleared")]
    public class MirrorMapClearedConditionSO : DialogueConditionSO
    {
        /// <summary>
        /// 判断梦境地图是否已经通关。
        /// </summary>
        public override bool IsMet()
        {
            return GameManager.Instance.GameSaveData.IsClearMirrorMap;
        }
    }
}
