using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

using DreamSenker.Characters;

//class是引用类型的约束 并不是单指类
//接口也是一个引用类型
//这里是为了可以支持as运算符
//继承BaseComponent表示这个类是一个可以复用的功能组件 T来指定该组件需要的Config
//不需要任何Config的话 指定默认的IConfig即可

namespace DreamSenker.Characters.Player
{
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
}
