using UnityEngine;

namespace PlayArk.StateMachine
{
    public class StateMachineController : MonoBehaviour
    {
        [SerializeField] private StateMachine _stateMachine;
        [Tooltip("是否在 Start 时自动启动状态机")]
        [SerializeField] private bool _playOnStart = true;

        private bool _isRunning;
        private bool _isBound;
    
        public StateMachine StateMachine => _stateMachine;
        public bool IsRunning => _isRunning;

        protected virtual void Awake()
        {
            //克隆一份用于运行时的资源实例
            if (_stateMachine != null)
            {
                _stateMachine = _stateMachine.Clone();
            }
        }

        protected virtual void Start()
        {
            if (_playOnStart)
            {
                StartMachine();
            }
        }

        protected virtual void Update()
        {
            if (!_isRunning || _stateMachine == null)
            {
                return;
            }

            //驱动状态轮询
            _stateMachine.LogicUpdate();
        }

        private void FixedUpdate()
        {
            if (!_isRunning || _stateMachine == null)
            {
                return;
            }

            _stateMachine.PhysicsUpdate();
        }

        /// <summary>
        /// 启动状态机，绑定一次运行时数据并进入入口状态。
        /// </summary>
        public void StartMachine()
        {
            if (_isRunning || _stateMachine == null)
            {
                return;
            }

            if (!_isBound)
            {
                _stateMachine.Bind(this);
                _isBound = true;
            }

            //开始执行状态机
            _stateMachine.MachineEnter();
            _isRunning = true;
        }

        /// <summary>
        /// 停止状态机，先退出当前状态再停止 Update / FixedUpdate 驱动。
        /// </summary>
        public void StopMachine()
        {
            if (!_isRunning || _stateMachine == null)
            {
                return;
            }

            _stateMachine.MachineExit();
            _isRunning = false;
        }

        /// <summary>
        /// Switches the active state in the StateMachine to the specified state.
        /// 是状态切换的对外窗口
        /// </summary>
        public void TransitionToState(string targetStateID)
        {
            _stateMachine.TransitionToState(targetStateID);
        }
    }
}
