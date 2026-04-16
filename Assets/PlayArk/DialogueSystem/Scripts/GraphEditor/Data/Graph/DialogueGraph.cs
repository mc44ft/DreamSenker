using PlayArk.GraphCore.Data;
using System;
using PlayArk.DialogueSystem.Data.Nodes;
using PlayArk.GraphCore;
using UnityEditor;
using UnityEngine;
namespace PlayArk.DialogueSystem.Data
{
    /// <summary>
    /// 对话图就相当于一个顺序执行器
    /// 可以顺序执行多张图
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueGraph_", menuName = "PlayArk Assets/GraphCore/DialogueGraph")]
    public class DialogueGraph : GraphCoreGraph<DialogueNodeBase, GraphCoreEdge>
    {
        /// <summary>
        /// 图完成后执行的回调
        /// 参数1：图是否执行成功
        /// 参数2：该对话是否应该保存
        /// </summary>
        private Action<bool, bool> _onFinished;
        private DialogueNodeBase _currentNode;

        [SerializeField, HideInInspector]
        private DialogueNodeEntry _nodeEntry;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinished">图完成后执行的回调</param>
        public void Initialize(Action<bool, bool> onFinished)
        {
            //初始化图中的全部节点
            foreach (DialogueNodeBase node in GetNodes())
            {
                node.Initialize(OnNodeFinished);
            }
            //设置图完成之后的回调
            _onFinished = onFinished;
        }
        /// <summary>
        /// 节点执行完成的回调
        /// </summary>
        /// <param name="isSuccess"></param>
        private void OnNodeFinished(bool isSuccess)
        {
            if (isSuccess)
            {
                //获取下一个节点
                var edge = GetEdgeByPortID(_currentNode.GetNextPortID());
                if (edge != null)
                    _currentNode = GetNodeByID(edge?.ConnectionNodeID);
                else
                    _currentNode = null;
                //执行下一个节点
                ExecuteCurrentNode();
            }
            else
            {
                //节点执行失败 -> 图执行失败
                _onFinished?.Invoke(false, false);
            }
        }
        /// <summary>
        /// 获取入口节点
        /// </summary>
        /// <returns></returns>
        private DialogueNodeBase GetEntranceNode()
        {
            return _nodeEntry;
        }
        private void ExecuteCurrentNode()
        {
            if (_currentNode != null)
            {
                _currentNode.Execute();
            }
            else
            {
                //图执行结束
                _onFinished?.Invoke(true, true);
            }
        }
        //图执行
        public void Execute()
        {
            _currentNode = GetEntranceNode();
            ExecuteCurrentNode();
        }
        /// <summary>
        /// 该方法允许节点强行结束图执行 并指定是否保存对话记录
        /// </summary>
        /// <param name="shouldSave"></param>
        public void ForceEnd(bool shouldSave)
        {
            _onFinished?.Invoke(true, shouldSave);
            _currentNode = null;
        }

        protected override void OnCreateDefaultNode()
        {
            //生成默认入口节点
            if (_nodeEntry == null)
            {
                _nodeEntry = CreateNode(typeof(DialogueNodeEntry), Vector2.zero) as DialogueNodeEntry;
                AddNode(_nodeEntry);
            }
        }
    }

}
