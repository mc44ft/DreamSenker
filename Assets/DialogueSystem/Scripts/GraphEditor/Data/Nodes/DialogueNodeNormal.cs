using System;
using UnityEngine;
using DialogueSystem.UI;
namespace DialogueSystem.Data
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
            EventCenter.Instance.AddEventListener<EmptyEventArgs>(E_EventType.Dialogue_ContentNext, OnTextNext);
            _index = 0;
            //自己先触发一次 直接处理第一句话
            EventCenter.Instance.EventTrigger(E_EventType.Dialogue_ContentNext, this, new EmptyEventArgs());
        }
        private void OnTextNext(object eventCenter, EmptyEventArgs args)
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
            EventCenter.Instance.RemoveEventListener<EmptyEventArgs>(E_EventType.Dialogue_ContentNext, OnTextNext);
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

