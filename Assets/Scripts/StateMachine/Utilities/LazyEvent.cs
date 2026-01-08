using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 为什么需要LazyEvent？
/// 主要是为了配合状态机系统
/// 因为状态机的检查是“轮询”的 即每一帧的Tick里去检查所有转换条件是否满足
/// 而事件触发是随机的
/// 举个例子 如果“受伤”这个事件在帧开始时触发并结束了，而状态机的逻辑刚好在帧末尾才去检查条件
/// 或者说状态机在帧开始检测逻辑 而受伤时间发生在帧末尾
/// 就会导致错过这个事件
/// LazyEvent让这个事件维持了一个极短的时间 保证了状态机不会错过该时间
/// </summary>
[Serializable]
public class LazyEvent : UnityEvent
{
    /// <summary>
    /// 记录维持的时间
    /// </summary>
    private const float _flagResetTime = 0.001f;

    /// <summary>
    /// 记录事件刚刚是否被触发
    /// </summary>
    private bool _wasInvoked = false;

    public new IEnumerator Invoke()
    {
        base.Invoke();
        _wasInvoked = true;
        yield return new WaitForSeconds(_flagResetTime);
        _wasInvoked = false;
    }
    public bool WasInvoked()
        => _wasInvoked;
}

[Serializable]
public class LazyEvent<T> : UnityEvent<T>
{
    /// <summary>
    /// 记录维持的时间
    /// </summary>
    private const float _flagResetTime = 0.001f;

    /// <summary>
    /// 记录事件刚刚是否被触发
    /// </summary>
    private bool _wasInvoked = false;

    public new IEnumerator Invoke(T value)
    {
        base.Invoke(value);
        _wasInvoked = true;
        yield return new WaitForSeconds(_flagResetTime);
        _wasInvoked = false;
    }
    public bool WasInvoked()
        => _wasInvoked;
}
