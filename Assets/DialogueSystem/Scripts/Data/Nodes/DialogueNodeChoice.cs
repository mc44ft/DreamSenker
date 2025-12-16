using System;
using UnityEngine;
using XNode;
namespace DialogueSystem.Data
{
    public class DialogueNodeChoice : DialogueNodeBase
    {
        
        [Space(10)]
        [Header("CHOICE DETAILS")]
        /// <summary>
        /// 动态端口 有几组数据就添加几组端口，索引对应连接即可
        /// 实在搞不懂怎么在数据数组中加入端口
        /// </summary>
        //[Output(dynamicPortList = true)] public Connection[] ChoiceOutput;
        [SerializeField] private int DefaultSelectIndex = 0;
        /// <summary>
        /// 直接将这个数组作为一个动态端口
        /// </summary>
        [Output(dynamicPortList = true)] public ChoiceData[] ChoiceDataArray;

        
        /// <summary>
        /// 玩家最终选择的节点索引
        /// </summary>
        private int _resultIndex;
        protected override void OnExecute()
        {
            //初始化并显示选项框
            DialogueManager.Instance.ShowDialogueChoicesSection(ChoiceDataArray, DefaultSelectIndex);
            EventCenter.Instance.AddEventListener<IntEventArgs>(E_EventType.Dialogue_ChoiceClick, OnChoiceClick);
        }

        private void OnChoiceClick(object eventSender, IntEventArgs args)
        {
            OnChoiceClick(args.Value);
        }

        private void OnChoiceClick(int index)
        {
            _resultIndex = index;
            Finished();
        }

        protected override void OnFinished()
        {
            EventCenter.Instance.RemoveEventListener<IntEventArgs>(E_EventType.Dialogue_ChoiceClick, OnChoiceClick);
            DialogueManager.Instance.HideDialogueChoicesSection();
        }
        /// <summary>
        /// 选项节点特有的返回下一个节点的方法 完全重写父类方法
        /// </summary>
        /// <returns></returns>
        public override DialogueNodeBase GetNextNode()
        {
            if (ChoiceDataArray == null || ChoiceDataArray.Length == 0)
                return null;
            if (_resultIndex < 0 || _resultIndex >= ChoiceDataArray.Length)
            {
                Debug.LogWarning("选项索引越界！");
                return null;
            }

            string portName = $"ChoiceDataArray {_resultIndex}";
            NodePort outputPort = GetOutputPort(portName);
            if (outputPort == null || !outputPort.IsConnected)
            {
                Debug.LogWarning("选项未连接后续节点");
                return null;
            }

            return outputPort.Connection.node as DialogueNodeBase;
        }

        [Serializable]
        public class ChoiceData
        {
            public GameObject ButtonPrefab;
            public string Content;
        }
    }
}

