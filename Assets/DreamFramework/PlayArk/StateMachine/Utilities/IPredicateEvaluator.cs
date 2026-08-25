using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    public interface IPredicateEvaluator
    {
        /// <summary>
        /// 判断是否满足谓词条件
        /// </summary>
        /// <returns>bool? 为可空类型 多了一个判断的状态 就是不知道
        /// 也就是说可以返回三种值true false null
        /// 如果具体的逻辑类里不负责监视该条件 则返回null</returns>
        bool? Evaluate(EPredicate predicate, string[] parameters);
    }
}