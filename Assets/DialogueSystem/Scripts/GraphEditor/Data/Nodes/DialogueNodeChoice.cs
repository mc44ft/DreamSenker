using PlayArk.GraphCore.Data;
using System;
using System.Linq;
using UnityEngine;
namespace DialogueSystem.Data
{
    [NodeMenuItem("DialogueNodeChoice")]
    public class DialogueNodeChoice : DialogueNodeBase
    {
        
        [Space(10)]
        [Header("CHOICE DETAILS")]
        /// <summary>
        /// 默认选择的选项索引 用于显示默认的光标位置
        /// </summary>
        [SerializeField] private int _defaultSelectIndex = 0;

        
        /// <summary>
        /// 玩家最终选择的节点索引
        /// </summary>
        private int _resultIndex;
        protected override void OnExecute()
        {
            //获取所有的选项数据
            ChoiceData[] choiceDatas = _outputPorts.Select(port => (port as DialogueChoicePort).ChoiceData).ToArray();
            //初始化并显示选项框
            DialogueManager.Instance.ShowDialogueChoicesSection(choiceDatas, _defaultSelectIndex);
            //获取玩家选择的选项索引
            EventCenter.Instance.AddEventListener<IntEventArgs>(E_EventType.Dialogue_ChoiceClick, OnChoiceClick);
        }

        private void OnChoiceClick(object eventSender, IntEventArgs args)
        {
            ChoiceClick(args.Value);
        }

        private void ChoiceClick(int index)
        {
            _resultIndex = index;
            Finished();
        }

        protected override void OnFinished()
        {
            EventCenter.Instance.RemoveEventListener<IntEventArgs>(E_EventType.Dialogue_ChoiceClick, OnChoiceClick);
            DialogueManager.Instance.HideDialogueChoicesSection();
        }
        protected override GraphCorePort CreatePortInstance()
        {
            //替换Port为DialogueChoicePort
            return new DialogueChoicePort();
        }
        public override string GetNextPortID()
        {
            if (_resultIndex < 0 || _resultIndex >= _outputPorts.Count)
            {
                Debug.LogWarning("选项索引越界！");
                return null;
            }

            return _outputPorts[_resultIndex].GetUniqueID();
        }
        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            base.Init(uniqueID, viewPosition);
            SetTitle("Choice Node");
        }

    }
    [Serializable]
    public class DialogueChoicePort : GraphCorePort
    {
        public ChoiceData ChoiceData;
    }
    [Serializable]
    public class ChoiceData
    {
        public GameObject ButtonPrefab;
        public string Content;
    }
}

