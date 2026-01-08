using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayArk.GraphCore.Data;
using PlayArk.StateMachine.Utilities;
using Unity.VisualScripting;
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    /// <summary>
    /// 合取范式（CNF）就是一个规则 外层全是与（and）内层全是或（or）
    /// </summary>
    [System.Serializable]
    public class Condition
    {
        [SerializeField] private Disjunction[] _and;

        public Condition Clone()
        {
            Condition clone = new Condition()
            {
                _and = CloneArray()
            };
            return clone;
        }

        private Disjunction[] CloneArray()
        {
            Disjunction[] clone = new Disjunction[_and.Length];
            for (int i = 0; i < _and.Length; i++)
            {
                clone[i] = _and[i].Clone();
            }

            return clone;
        }

        /// <summary>
        /// 所有项都满足 结果才满足
        /// </summary>
        public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
        {
            foreach (var disjunction in _and)
            {
                if (!disjunction.Check(evaluators))
                {
                    return false;
                }
            }

            return true;
        }
    }
}

/// <summary>
/// 析取项
/// </summary>
[System.Serializable]
public class Disjunction
{
    [SerializeField] private Predicate[] _or;

    public Disjunction Clone()
    {
        Disjunction clone = new Disjunction()
        {
            _or = CloneArray()
        };
        return clone;
    }

    private Predicate[] CloneArray()
    {
        Predicate[] clone = new Predicate[_or.Length];
        for (int i = 0; i < _or.Length; i++)
        {
            clone[i] = _or[i].Clone();
        }

        return clone;
    }

    /// <summary>
    /// 有一项满足 结果就满足
    /// </summary>
    public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
    {
        foreach (var predicate in _or)
        {
            if (predicate.Check(evaluators))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 这是原子命题
/// </summary>
[System.Serializable]
public class Predicate
{
    /// <summary>
    /// 这个枚举项就是谓词主体 就是那个P
    /// </summary>
    [SerializeField] private EPredicate _predicate;

    /// <summary>
    /// 这个是谓词参数 就是那个P(x) 的 x
    /// </summary>
    [SerializeField] private string[] _parameters;

    /// <summary>
    /// 否定 如果此项为真 会对判断结果取反
    /// </summary>
    [SerializeField] private bool _negate = false;

    public Predicate Clone()
    {
        Predicate clone = new Predicate()
        {
            _predicate = this._predicate,
            //string是特殊的引用类型 所有这里浅拷贝即可达到深拷贝的效果 当成值类型来用就可以了
            _parameters = this._parameters.ToArray(),
            _negate = this._negate
        };
        return clone;
    }

    public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
    {
        foreach (var evaluator in evaluators)
        {
            bool? result = evaluator.Evaluate(_predicate, _parameters);
            //如果是null 说明该判断不归该脚本管 跳过
            if (result == null)
            {
                continue;
            }

            if (result == _negate)
            {
                return false;
            }
        }

        return true;
    }
}