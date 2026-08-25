
using PlayArk.GraphCore.Data;
using UnityEngine;

namespace PlayArk.StateMachine
{
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

        /// <summary>
        /// 状态机退出入口，用于停止驱动前清理当前状态副作用。
        /// </summary>
        public void MachineExit()
        {
            _currentState?.Exit();
            _currentState = null;
        }

        public void LogicUpdate()
        {
            if (_currentState == null)
            {
                return;
            }

            _currentState.LogicUpdate();
            _anyState.LogicUpdate();
        }

        public void PhysicsUpdate()
        {
            if (_currentState == null)
            {
                return;
            }

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
                //这里State直接使用Instantiate进行拷贝，是因为State中没有任何外部引用，
                //_transitions只是一套缓存数据而已
                clone.AddNode(state.Clone());
            }

            foreach (var edge in _edges)
            {
                //连线要手动拷贝，因为它不是SO资源 不能用Instantiate方法克隆
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
