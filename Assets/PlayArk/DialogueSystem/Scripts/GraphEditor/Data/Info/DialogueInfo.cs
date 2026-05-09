using System;
using UnityEngine;
using UnityEngine.Serialization;

using DreamSenker.Shared;

namespace PlayArk.DialogueSystem.Data
{
    [Serializable]
    public class DialogueNpcTriggerInfo
    {
        public DialogueGraph GraphMain;
        public DialogueGraph GraphTail;

        [FormerlySerializedAs("GameCondition")] [Tooltip("需要判断的游戏条件")]
        public EGameCondition eGameCondition;
        public string guid;
    }
    [Serializable]
    public class DialogueOneTriggerInfo
    {
        public DialogueGraph GraphMain;

        [FormerlySerializedAs("GameCondition")] [Tooltip("需要判断的游戏条件")]
        public EGameCondition eGameCondition;
        public string guid;
    }
}
