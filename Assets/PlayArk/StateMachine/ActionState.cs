using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.GraphCore.Utilities;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

namespace PlayArk.StateMachine
{
    [NodeMenuItem("Action State")]
    public class ActionState : State
    {
        [SerializeField] private ActionData[] _onEnterActions;
        [SerializeField] private ActionData[] _onLogicUpdateActions;
        [SerializeField] private ActionData[] _onPhysicsUpdateActions;
        [SerializeField] private ActionData[] _onExitActions;
        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            base.Init(uniqueID, viewPosition);
            SetTitle("Action State");
        }

        public override void Enter()
        {
            base.Enter();
            DoActions(_onEnterActions);
        }

        public override void LogicUpdate()
        {
            base.LogicUpdate();
            DoActions(_onLogicUpdateActions);
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
            DoActions(_onPhysicsUpdateActions);
        }

        public override void Exit()
        {
            base.Exit();
            DoActions(_onExitActions);
        }

        private void DoActions(ActionData[] actions)
        {
            foreach (var actionSender in _controller.GetComponents<IAction>())
            {
                //保证不会执行被关闭的逻辑
                if (actionSender is Behaviour behaviour && !behaviour.isActiveAndEnabled)
                    continue;

                foreach (ActionData action in actions)
                {
                    actionSender.DoAction(action.action, action.parameters);
                }
            }
        
        }
        [Serializable]
        private class ActionData
        {
            /// <summary>
            /// 要执行的逻辑段
            /// </summary>
            public EAction action;
            /// <summary>
            /// 参数名称 主要是动画名
            /// </summary>
            public string[] parameters;
        }
    }
}
