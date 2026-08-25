using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 将父类设置为抽象类，避免直接实例化父类
/// 规定子类必须显式的实现构造函数私有化
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class BaseManager<T> where T : class//, new()
{
    #region 注意事项

    //写在前面
    //调用反射会带来性能开销
    //在工作中 即使不处理这些安全问题 只要不在外部随意new一个单例模式对象 就没问题
    //在实际开发中按需求来写 可以不必过于追求安全性

    //如何防止外部实例化？
    //1.基类设置为抽象类
    //2.规定子类必须显式的实现构造函数私有化
    //3.在基类中通过反射来调用私有构造函数实例化对象
    //利用Type中的 GetConstructor(约束条件，绑定对象，参数类型，参数修饰符)方法 来获取私有无参构造函数
    //ConstructorInfo constructor = typeof(T).GetConstructor(
    //BindingFlags.Instance | BindingFlags.NonPublic, //表示成员私有方法
    //null,                                           //表示不需要绑定对象
    //Type.EmptyTypes,                                //表示没有参数
    //null                                           )//表示没有参数修饰符

    //利用方法来获取单例模式
    //public static T GetInstance()
    //{
    //    if (instance == null)
    //    {
    //        instance = new T();
    //    }
    //    return instance;
    //}

    #endregion 注意事项

    private static T _instance;
    protected static readonly object _lockObj = new object(); //锁对象，用于线程安全 readonly表示这是一个只读的引用类型对象 不能去修改它

    //利用属性来获取单例模式
    public static T Instance
    {
        get
        {
            if (_instance == null)//先判空再锁 防止每次进来都锁线程 减少性能开销
            {
                lock (_lockObj)//避免多线程同时访问时 同时new一个单例模式对象
                {
                    if (_instance == null)//这个判空也是必要的 防止多个线程同时进入触发等待时 多次创建单例模式对象
                    {
                        //instance = new T();
                        Type type = typeof(T);
                        ConstructorInfo info = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                                    null,
                                                                    Type.EmptyTypes,
                                                                    null);
                        if (info != null)
                            _instance = info.Invoke(null) as T;
                        else
                            Debug.LogError(type.Name + " does not have a private constructor.");
                    }
                }
            }

            return _instance;
        }
    }
}