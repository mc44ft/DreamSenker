using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSeeker.Framework.Events
{
/// <summary>
/// 以事件结构体类型为键的全局静态事件总线。
/// </summary>
public static class EventBus
{
    /// <summary>
    /// 保存不同事件类型对应的订阅委托。
    /// </summary>
    private static readonly Dictionary<Type, Delegate> Handlers = new Dictionary<Type, Delegate>();

    /// <summary>
    /// 订阅指定类型的事件。
    /// </summary>
    public static void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        Type eventType = typeof(TEvent);
        if (Handlers.TryGetValue(eventType, out Delegate existingHandler))
        {
            Handlers[eventType] = Delegate.Combine(existingHandler, handler);
            return;
        }

        Handlers.Add(eventType, handler);
    }

    /// <summary>
    /// 取消订阅指定类型的事件。
    /// </summary>
    public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct
    {
        if (handler == null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        Type eventType = typeof(TEvent);
        if (!Handlers.TryGetValue(eventType, out Delegate existingHandler))
        {
            return;
        }

        Delegate remainingHandler = Delegate.Remove(existingHandler, handler);
        if (remainingHandler == null)
        {
            Handlers.Remove(eventType);
            return;
        }

        Handlers[eventType] = remainingHandler;
    }

    /// <summary>
    /// 向当前事件类型的所有订阅者发布消息。
    /// </summary>
    public static void Publish<TEvent>(TEvent eventData) where TEvent : struct
    {
        if (Handlers.TryGetValue(typeof(TEvent), out Delegate handler))
        {
            ((Action<TEvent>)handler).Invoke(eventData);
        }
    }

    /// <summary>
    /// 清除指定事件类型的全部订阅者。
    /// </summary>
    public static void Clear<TEvent>() where TEvent : struct
    {
        Handlers.Remove(typeof(TEvent));
    }

    /// <summary>
    /// 清除全部事件订阅者。
    /// </summary>
    public static void ClearAll()
    {
        Handlers.Clear();
    }

    /// <summary>
    /// 在运行环境重置时清理静态订阅，兼容关闭 Domain Reload 的编辑器配置。
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        ClearAll();
    }
}
}
