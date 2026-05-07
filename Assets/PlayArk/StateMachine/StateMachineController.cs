using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace PlayArk.StateMachine
{
    public class StateMachineController : MonoBehaviour
    {
        [SerializeField] private StateMachine _stateMachine;
    
        public StateMachine StateMachine => _stateMachine;

        protected virtual void Awake()
        {
            //克隆一份用于运行时的资源实例
            _stateMachine = _stateMachine.Clone();
        }

        protected virtual void Start()
        {
            _stateMachine.Bind(this);
            //开始执行状态机
            _stateMachine.MachineEnter();
        }

        protected virtual void Update()
        {
            //驱动状态轮询
            _stateMachine.LogicUpdate();
        }

        private void FixedUpdate()
        {
            _stateMachine.PhysicsUpdate();
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