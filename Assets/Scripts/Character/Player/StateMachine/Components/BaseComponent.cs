using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
//class是引用类型的约束 并不是单指类
//接口也是一个引用类型
//这里是为了可以支持as运算符
public abstract class BaseComponent<T> : MonoBehaviour, IPredicateEvaluator ,IComponent<T> where T : class, IConfig
{

    public void InjectionConfigBase(IConfig config)
    {
        if (config is T componentConfig)
        {
            InjectionConfig(componentConfig);
        }
    }
    public abstract void InjectionConfig(T moveConfig);
    public abstract bool? Evaluate(EPredicate predicate, string[] parameters);
}
