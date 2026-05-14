using System;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data
{
    [Serializable]
    public class DialogueNpcTriggerInfo
    {
        /// <summary>
        /// 首次触发时播放的对话图。
        /// </summary>
        public DialogueGraph GraphMain;
        /// <summary>
        /// 重复触发时播放的对话图。
        /// </summary>
        public DialogueGraph GraphTail;

        /// <summary>
        /// 需要全部满足的对话触发条件。
        /// </summary>
        public DialogueConditionSO[] Conditions;
        /// <summary>
        /// 对话触发记录ID。
        /// </summary>
        [ReadOnly] public string guid;
    }
    [Serializable]
    public class DialogueOneTriggerInfo
    {
        /// <summary>
        /// 一次性触发时播放的对话图。
        /// </summary>
        public DialogueGraph GraphMain;

        /// <summary>
        /// 需要全部满足的对话触发条件。
        /// </summary>
        public DialogueConditionSO[] Conditions;
        /// <summary>
        /// 对话触发记录ID。
        /// </summary>
        [ReadOnly] public string guid;
    }
}
