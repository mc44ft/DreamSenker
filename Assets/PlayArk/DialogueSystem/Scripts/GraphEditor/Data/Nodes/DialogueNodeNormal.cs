using System;
using DialogueSystem;
using UnityEngine;
using DreamSeeker.Framework.Events;
using PlayArk.DialogueSystem.Events;
using PlayArk.DialogueSystem.Runtime;
using PlayArk.GraphCore.Utilities;

namespace PlayArk.DialogueSystem.Data.Nodes
{
    [NodeMenuItem("DialogueNodeNormal")]
    public class DialogueNodeNormal : DialogueNodeBase
    {
        [Space(10)]
        [Header("DIALOGUE DETAILS")]
        [SerializeField] private DialogueData[] _dialogueDataArray;
        
        private int _index;

        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            base.Init(uniqueID, viewPosition);
            SetTitle("Normal");
        }

        protected override void OnExecute()
        {
            //在节点执行时 开启事件监听
            EventBus.Subscribe<DialogueAdvanceRequestedEvent>(OnTextNext);
            _index = 0;
            //自己先触发一次 直接处理第一句话
            EventBus.Publish(new DialogueAdvanceRequestedEvent());
        }
        /// <summary>
        /// 响应对话推进请求并输出下一条内容。
        /// </summary>
        private void OnTextNext(DialogueAdvanceRequestedEvent eventData)
        {
            if (_index < _dialogueDataArray.Length)
            {
                DialogueManager.Instance.PrintNextContent(_dialogueDataArray[_index]);
                _index++;
                return;
            }
            OnFinished();
        }
        protected override void Finished()
        {
            //节点完成后 结束事件监听
            EventBus.Unsubscribe<DialogueAdvanceRequestedEvent>(OnTextNext);
        }

        [Serializable]
        public class DialogueData
        {
            public string SpeakerName;
            [Multiline] public string Content;
            public DialogueText.E_DisplayType DisplayType = DialogueText.E_DisplayType.Typing;
            public bool CanQuickShow;
        }
    }
}
