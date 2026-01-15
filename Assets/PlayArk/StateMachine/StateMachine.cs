
using PlayArk.GraphCore.Data;
using UnityEngine;

namespace PlayArk.StateMachine
{
    [CreateAssetMenu(fileName = "StateMachine_", menuName = "PlayArk Assets/GraphCore/State Machine")]
    public class StateMachine : GraphCoreGraph<State, StateTransitionEdge>
    {
        [SerializeField, HideInInspector] private EntryState _entryState;
        [SerializeField, HideInInspector] private AnyState _anyState;

        private State _currentState;

        public void Bind(StateMachineController controller)
        {
            //将状态机控制器绑定到所有状态上
            foreach (var node in _nodes)
            {
                node.Bind(controller);
            }
            //将状态机控制器绑定到所有连线上
            foreach (var edge in _edges)
            {
                edge.Bind(controller);
            }
        }
        /// <summary>
        /// 状态机启动入口
        /// </summary>
        public void MachineEnter()
        {
            TransitionToState(_entryState.Transitions[0].ConnectionNodeID);
        }

        public void LogicUpdate()
        {
            _currentState.LogicUpdate();
            _anyState.LogicUpdate();
        }

        public void PhysicsUpdate()
        {
            _currentState.PhysicsUpdate();
        }

        public void TransitionToState(string targetStateID)
        {
            _currentState?.Exit();
            _currentState = GetNodeByID(targetStateID);
            _currentState?.Enter();
        }
        /// <summary>
        /// 克隆一份运行时的状态机实例
        /// </summary>
        /// <returns></returns>
        public StateMachine Clone()
        {
            //这个Instantiate的操作是一个深拷贝
            StateMachine clone = Instantiate(this);
        
            //清空内部字段的引用 因为clone过来 这个克隆体字段的引用还是原来的
            clone._nodes.Clear();
            clone._edges.Clear();
            clone._nodeLookup.Clear();
            clone._edgeLookup.Clear();
            clone._portToEdgeLookup.Clear();

            foreach (var state in _nodes)
            {
                clone.AddNode(state.Clone());
            }

            foreach (var edge in _edges)
            {
                clone.AddEdge(edge.Clone());
            }
            //重新定位两个特殊状态
            clone._entryState = clone.GetNodeByID(_entryState.GetUniqueID()) as EntryState;
            clone._anyState = clone.GetNodeByID(_anyState.GetUniqueID()) as AnyState;
            return clone;
        }
        protected override void OnCreateDefaultNode()
        {
            if (_entryState == null)
            {
                _entryState = CreateNode(typeof(EntryState), new Vector2(100, 0)) as EntryState;
                AddNode(_entryState);
            }

            if (_anyState == null)
            {
                _anyState = CreateNode(typeof(AnyState), new Vector2(100, 150)) as AnyState;
                AddNode(_anyState);
            }
        }
    }
}