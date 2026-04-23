using System;
using System.Collections.Generic;
using UnityEngine.Events;
//该类用于里氏替换 可以用一个外部不能new的抽象类 也可以使用接口
public abstract class EventInfoBase
{ }
//更新：
//加入了事件触发者参数
//删除了无参数的事件（使用EventArgs.Empty作为无参的 “参数”)
public class EventInfo<T> : EventInfoBase where T : struct, IEventArgs
{
    public UnityAction<object, T> Action;

    public EventInfo(UnityAction<object, T> action)
    {
        Action += action;
    }
}
public class EventCenter : BaseManager<EventCenter>
{
    private readonly Dictionary<E_EventType, EventInfoBase> _eventDic = new Dictionary<E_EventType, EventInfoBase>();//事件字典

    private EventCenter()
    { }

    /// <summary>
    /// 添加事件监听者（有参）
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="func"></param>
    public void AddEventListener<T>(E_EventType eventName, UnityAction<object, T> func) where T : struct, IEventArgs
    {
        if (_eventDic.TryGetValue(eventName, out EventInfoBase eventInfoBase) && 
            eventInfoBase is EventInfo<T> eventInfo)
        {
            eventInfo.Action += func;
        }
        else
        {
            _eventDic.Add(eventName, new EventInfo<T>(func));
        }
    }

    /// <summary>
    /// 移除事件监听者（有参）
    /// 这里一定要记得移除 不然会造成内存泄漏
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="action"></param>
    public void RemoveEventListener<T>(E_EventType eventName, UnityAction<object, T> action) where T : struct, IEventArgs
    {
        if (_eventDic.TryGetValue(eventName, out EventInfoBase eventInfoBase) &&
            eventInfoBase is EventInfo<T> eventInfo)
        {
            eventInfo.Action -= action;
        }
    }
    /// <summary>
    /// 触发（分发）事件（有参）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="eventName"></param>
    /// <param name="eventSender">事件触发者</param>
    /// <param name="info"></param>
    public void EventTrigger<T>(E_EventType eventName, object eventSender, T info) where T : struct, IEventArgs
    {
        if(_eventDic.TryGetValue(eventName, out EventInfoBase eventInfoBase) &&
           eventInfoBase is EventInfo<T> eventInfo)
        {
            eventInfo.Action?.Invoke(eventSender, info);
        }
    }
    /// <summary>
    /// 清空所有事件监听
    /// </summary>
    public void ClearAllListeners()
    {
        _eventDic.Clear();
    }
    /// <summary>
    /// 清除指定事件监听
    /// </summary>
    /// <param name="eventName"></param>
    public void ClearListener(E_EventType eventName)
    {
        _eventDic.Remove(eventName);
    }
}