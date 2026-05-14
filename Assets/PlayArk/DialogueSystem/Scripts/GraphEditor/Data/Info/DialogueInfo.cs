using System;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data
{
    [Serializable]
    public abstract class DialogueTriggerInfoBase : ISerializationCallbackReceiver
    {
        /// <summary>
        /// 对话触发记录ID。
        /// </summary>
        [ReadOnly] public string guid;

        /// <summary>
        /// 创建对话触发信息时生成唯一ID。
        /// </summary>
        protected DialogueTriggerInfoBase()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 序列化前确保唯一ID存在。
        /// </summary>
        public void OnBeforeSerialize()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 反序列化后确保唯一ID存在。
        /// </summary>
        public void OnAfterDeserialize()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 在ID为空时生成唯一ID。
        /// </summary>
        private void EnsureGuid()
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }
    }

    [Serializable]
    public class DialogueNpcTriggerInfo : DialogueTriggerInfoBase
    {
        /// <summary>
        /// 条件满足时播放的对话图。
        /// </summary>
        public DialogueGraph GraphMain;

        /// <summary>
        /// 需要全部满足的对话触发条件。
        /// </summary>
        public DialogueConditionSO[] Conditions;
    }
    [Serializable]
    public class DialogueOneTriggerInfo : DialogueTriggerInfoBase
    {
        /// <summary>
        /// 一次性触发时播放的对话图。
        /// </summary>
        public DialogueGraph GraphMain;

        /// <summary>
        /// 需要全部满足的对话触发条件。
        /// </summary>
        public DialogueConditionSO[] Conditions;
    }
}
