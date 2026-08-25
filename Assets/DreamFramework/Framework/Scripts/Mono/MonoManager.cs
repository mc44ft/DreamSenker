using UnityEngine.Events;

/// <summary>
/// 公共Mono模块管理器
/// </summary>
public class MonoManager : SingletonAutoMono<MonoManager>
{
    #region 注意事项
    //前言：
    //不要陷入“有框架就是好”的思维定式 框架应该按需使用
    //正常的帧更新逻辑正常写在Update里就好

    //功能：
    //1.可以用来让不继承Mono的类也能实现帧更新和协程
    //2.还可以用于管理继承Mono的类的帧更新和协程 避免多次调用不同的Update造成性能开销 而是把很多逻辑放在一个MonoMgr中统一管理
    //3.可以实现精确的执行顺序控制（后续优化：可以加入优先级参数，完全实现由代码控制的执行顺序）
    //4.集中管理所有Update逻辑（只需一个if语句，即可实现所有订阅的Update全部暂停）
    //5.如果有成千上万个Update，使用这种模式会带来一定的性能提升（在现代Unity中，正常数量的Update已经不能造成性能瓶颈，不需要过多考虑）


    //注意：
    //关于协程，不需要再次封装开启或者关闭协程的方法 直接使用MonoBehaviour内的协程开启或关闭方法即可
    //在不继承Mono的类中开启协程时，不能使用传方法名字符串的方式开启 传方法名的方式只能在继承Mono的类中被找到

    #endregion 注意事项

    private event UnityAction _updateEvent;

    private event UnityAction _fixedUpdateEvent;

    private event UnityAction _lateUpdateEvent;

    /// <summary>
    /// 添加Update监听事件
    /// </summary>
    /// <param name="action"></param>
    public void AddUpdateListener(UnityAction action)//Listener-监听者
    {
        if (action != null)
        {
            _updateEvent += action;
        }
    }

    /// <summary>
    /// 移除Update监听事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveUpdateListener(UnityAction action)
    {
        if (action != null)
        {
            _updateEvent -= action;
        }
    }

    /// <summary>
    /// 添加FixedUpdate监听事件
    /// </summary>
    /// <param name="action"></param>
    public void AddFixedUpdateListener(UnityAction action)
    {
        if (action != null)
        {
            _fixedUpdateEvent += action;
        }
    }

    /// <summary>
    /// 移除FixedUpdate监听事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveFixedUpdateListener(UnityAction action)//添加Update监听事件
    {
        if (action != null)
        {
            _fixedUpdateEvent -= action;
        }
    }

    /// <summary>
    /// 添加LateUpdate监听事件
    /// </summary>
    /// <param name="action"></param>
    public void AddLateUpdateListener(UnityAction action)
    {
        if (action != null)
        {
            _lateUpdateEvent += action;
        }
    }

    /// <summary>
    /// 移除LateUpdate监听事件
    /// </summary>
    /// <param name="action"></param>
    public void RemoveLateUpdateListener(UnityAction action)
    {
        if (action != null)
        {
            _lateUpdateEvent -= action;
        }
    }

    private void Update()//帧更新
    {
        _updateEvent?.Invoke();
    }

    private void FixedUpdate()//物理帧更新
    {
        _fixedUpdateEvent?.Invoke();
    }

    private void LateUpdate()//晚于Update的帧更新
    {
        _lateUpdateEvent?.Invoke();
    }
}