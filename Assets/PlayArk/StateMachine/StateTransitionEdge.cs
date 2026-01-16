using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.GraphCore;
using PlayArk.GraphCore.Data;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

namespace PlayArk.StateMachine
{
    [Serializable]
    public class StateTransitionEdge : GraphCoreEdge
    {
        /// <summary>
        /// 用于条件判断的合取范式
        /// </summary>
        [SerializeField] private Condition _condition;

        private StateMachineController _controller;

        public void Bind(StateMachineController controller)
        {
            _controller = controller;
        }
        public StateTransitionEdge Clone()
        {
            StateTransitionEdge clone = new StateTransitionEdge()
            {
                _condition = this._condition.Clone(),
            };
            return clone;
        }

        public bool Check()
        {
            //检查对象身上是否满足转换条件
            return _condition.Check(_controller.GetComponents<IPredicateEvaluator>());
        }
    }
}