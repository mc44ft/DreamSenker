using UnityEngine;

namespace PlayArk.DialogueSystem.Data
{
    /// <summary>
    /// 对话触发条件基类。
    /// </summary>
    public abstract class DialogueConditionSO : ScriptableObject
    {
        /// <summary>
        /// 判断当前对话条件是否满足。
        /// </summary>
        public abstract bool IsMet();
    }
}
