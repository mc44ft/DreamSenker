
using PlayArk.GraphCore.Data;
using System;
using System.Linq;
using UnityEngine;
namespace DialogueSystem.Data
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinished">图完成后执行的回调</param>
        public void Initialize(Action<bool, bool> onFinished)
        {
            //初始化图中的全部节点
            foreach (DialogueNodeBase node in GetNodes())
            {
                Debug.Log("初始化节点");
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
        private DialogueNodeBase GetEntranceNode()
        {
            //FirstOrDefault LINQ的一个方法 意思是找和条件相同的第一个元素 如果找不到 就返回默认值 引用对象的默认值就是null
            //GetInputPort("Input") 得到该节点上名为 Input 的输入端口
            //并且该输入端口的连接线为0 就是没有被任何节点连接
            //return GetNodes().FirstOrDefault(x => (x as DialogueNodeBase).GetInputPort("Input").ConnectionCount == 0) as DialogueNodeBase;
            return GetNodes().First();
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
    }

}
