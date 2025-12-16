
using System;
using System.Linq;
using UnityEngine;
using XNode;
namespace DialogueSystem.Data
{
    /// <summary>
    /// 对话图就相当于一个顺序执行器
    /// 可以顺序执行多张图
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueGraph_", menuName = "NodeGraph/DialogueGraph")]
    public class DialogueGraph : NodeGraph
    {
        /// <summary>
        /// 图完成后执行的回调
        /// 参数1：图是否执行成功
        /// 参数2：该对话是否应该保存
        /// </summary>
        private Action<bool, bool> m_onFinished;
        private DialogueNodeBase m_currentNode;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="onFinished">图完成后执行的回调</param>
        public void Initialize(Action<bool, bool> onFinished)
        {
            //初始化图中的全部节点
            foreach (DialogueNodeBase node in nodes)
            {
                node.Initialize(OnNodeFinished);
            }
            //设置图完成之后的回调
            m_onFinished = onFinished;
        }
        /// <summary>
        /// 节点执行完成的回调
        /// </summary>
        /// <param name="isSuccess"></param>
        private void OnNodeFinished(bool isSuccess)
        {
            if (isSuccess)
            {
                //执行下一个节点
                m_currentNode = m_currentNode.GetNextNode();
                ExecuteCurrentNode();
            }
            else
            {
                //节点执行失败 -> 图执行失败
                m_onFinished?.Invoke(false, false);
            }
        }
        private DialogueNodeBase GetEntranceNode()
        {
            //FirstOrDefault LINQ的一个方法 意思是找和条件相同的第一个元素 如果找不到 就返回默认值 引用对象的默认值就是null
            //GetInputPort("Input") 得到该节点上名为 Input 的输入端口
            //并且该输入端口的连接线为0 就是没有被任何节点连接
            return nodes.FirstOrDefault(x => (x as DialogueNodeBase).GetInputPort("Input").ConnectionCount == 0) as DialogueNodeBase;
        }
        private void ExecuteCurrentNode()
        {
            if (m_currentNode != null)
            {
                m_currentNode.Execute();
            }
            else
            {
                //图执行结束
                m_onFinished?.Invoke(true, true);
            }
        }
        //图执行
        public void Execute()
        {
            m_currentNode = GetEntranceNode();
            ExecuteCurrentNode();
        }
        /// <summary>
        /// 该方法允许节点强行结束图执行 并指定是否保存对话记录
        /// </summary>
        /// <param name="shouldSave"></param>
        public void ForceEnd(bool shouldSave)
        {
            m_onFinished?.Invoke(true, shouldSave);
            m_currentNode = null;
        }
    }

}
