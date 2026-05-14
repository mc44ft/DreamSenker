using System;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data
{
    [Serializable]
    public abstract class DialogueTriggerInfoBase
    {
        public string title;
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
    public class DialogueNpcTriggerInfo : DialogueTriggerInfoBase
    {
    }

    [Serializable]
    public class DialogueOneTriggerInfo : DialogueTriggerInfoBase, ISerializationCallbackReceiver
    {
        /// <summary>
        /// 一次性对话触发记录ID。
        /// </summary>
        [ReadOnly] public string guid;

        /// <summary>
        /// 创建一次性对话触发信息时生成唯一ID。
        /// </summary>
        public DialogueOneTriggerInfo()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 序列化前确保一次性触发ID存在。
        /// </summary>
        public void OnBeforeSerialize()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 反序列化后确保一次性触发ID存在。
        /// </summary>
        public void OnAfterDeserialize()
        {
            EnsureGuid();
        }

        /// <summary>
        /// 在一次性触发ID为空时生成唯一ID。
        /// </summary>
        private void EnsureGuid()
        {
            if (string.IsNullOrWhiteSpace(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }
    }
}
