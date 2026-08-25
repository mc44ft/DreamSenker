using DreamSeeker.Conditions;
using UnityEngine;

using DreamSeeker.QuestSystem.Data;

namespace PlayArk.DialogueSystem.Data
{
    /// <summary>
    /// 对话触发条件基类。
    /// </summary>
    public abstract class ConditionSO : ScriptableObject
    {
        public abstract bool IsMet(ConditionContext context);
    }
}
