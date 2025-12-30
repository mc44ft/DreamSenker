using PlayArk.GraphCore.Data;
using System;
namespace DialogueSystem.Data
{
    public abstract class DialogueNodeBase : GraphCoreNode<GraphCorePort>
    {
        public E_NodeState CurrentState { get; protected set; }
        protected Action<bool> _onFinished;

        public virtual void Initialize(Action<bool> onFinished)
        {
            _onFinished = onFinished;
            CurrentState = E_NodeState.Wating;
        }
        /// <summary>
        /// 当前节点执行完毕后 将指定的端口ID返回给图对象
        /// 由图对象来根据ID查找连线 并前往下一个节点
        /// </summary>
        /// <returns></returns>
        public virtual string GetNextPortID()
        {
            return _outputPorts[0].GetUniqueID();
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
            _onFinished?.Invoke(true);
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