using UnityEngine;

using DreamSeeker.QuestSystem.Data;

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

        /// <summary>
        /// 使用任务上下文判断当前对话条件是否满足；非任务条件默认忽略任务上下文。
        /// </summary>
        public virtual bool IsMet(QuestDefinitionSO questDefinition)
        {
            return IsMet();
        }
    }
}
