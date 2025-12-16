using System;
using UnityEngine;
using XNode;
namespace DialogueSystem.Data
{
    public abstract class DialogueNodeBase : Node
    {
        public E_NodeState CurrentState { get; protected set; }
        protected Action<bool> m_onFinished;

        //输入和输出点
        //[Input] 和 [Output] 是xNode内部定义的特性
        [Input] public Connection Input;
        [Output] public Connection Output;
        protected override void Init()
        {
            base.Init();
            //让该节点不会显示在图的子级中 保持图的干净
            this.hideFlags = HideFlags.HideInHierarchy;
        }
        public virtual void Initialize(Action<bool> onFinished)
        {
            m_onFinished = onFinished;
            CurrentState = E_NodeState.Wating;
        }
        /// <summary>
        /// 输入端口为参数
        /// 输出端口为结果
        /// 此函数返回计算的结果 在该函数内进行计算
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public override object GetValue(NodePort port)
        {
            return null;
        }
        /// <summary>
        /// 当前节点执行完毕后 获取下一个节点
        /// </summary>
        /// <returns></returns>
        public virtual DialogueNodeBase GetNextNode()
        {
            NodePort outputPort = GetOutputPort("Output");
            if (outputPort == null || !outputPort.IsConnected)
                return null;
            return outputPort.Connection.node as DialogueNodeBase;
        }
        public void Execute()
        {
            if (CurrentState != E_NodeState.Wating) return;
            CurrentState = E_NodeState.Executing;

            OnExecute();
        }
        /// <summary>
        /// 节点执行完成时手动调用此方法
        /// </summary>
        public void Finished()
        {
            OnFinished();

            CurrentState = E_NodeState.Finished;

            //执行下一个节点
            m_onFinished?.Invoke(true);
        }
        protected abstract void OnExecute();
        protected abstract void OnFinished();
        [Serializable]
        public class Connection { }
    }
}
public enum E_NodeState
{
    Wating,
    Executing,
    Finished
}