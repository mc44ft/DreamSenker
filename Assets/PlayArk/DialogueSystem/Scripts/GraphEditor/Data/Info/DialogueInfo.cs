using System;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data
{
    [Serializable]
    public class DialogueNpcTriggerInfo
    {
        public DialogueGraph GraphMain;
        public DialogueGraph GraphTail;

        [Tooltip("需要判断的游戏条件")]
        public E_GameCondition GameCondition;
        public string guid;
    }
    [Serializable]
    public class DialogueOneTriggerInfo
    {
        public DialogueGraph GraphMain;

        [Tooltip("需要判断的游戏条件")]
        public E_GameCondition GameCondition;
        public string guid;
    }
}
