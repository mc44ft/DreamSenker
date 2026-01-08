using System;
using System.Linq;
using PlayArk.GraphCore;
using PlayArk.GraphCore.Data;

namespace PlayArk.StateMachine
{
    public class State : GraphCoreNode<GraphCorePort>
    {
        /// <summary>
        /// 用于提示该状态是否已启动
        /// </summary>
        protected bool _started = false;
        protected StateMachineController _controller;
    
        protected StateTransitionEdge[]  _transitions;
    
        /// <summary>
        /// 这里不影响基础节点图的逻辑 在这里给到StateMachine的图资源
        /// </summary>
        public void Bind(StateMachineController stateMachineController)
        {
            _controller = stateMachineController;
            //填充节点的出度信息
            _transitions = _controller.StateMachine.GetEdges().Where(edge => edge.RootNodeID == GetUniqueID()).ToArray();
        }
        public State Clone()
            => Instantiate(this);
    
        public virtual void Enter()
        {
            _started = true;
        }

        public virtual void LogicUpdate()
        {
            //在这里进行状态轮询的条件判断
            CheckTransitions();
        }
    
        public virtual void PhysicsUpdate() { }

        public virtual void Exit()
        {
            _started = false;
        }
        /// <summary>
        /// 检测该节点所有的转换条件 如果通过则转换到对应状态
        /// </summary>
        private void CheckTransitions()
        {
            foreach (var transition in _transitions)
            {
                bool success = transition.Check();
                if (success)
                {
                    _controller.TransitionToState(transition.ConnectionNodeID);
                }
            }
        }
    }
}