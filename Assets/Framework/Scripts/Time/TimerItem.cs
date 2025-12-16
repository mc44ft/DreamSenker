using UnityEngine.Events;

/// <summary>
/// 时间单位：毫秒
/// </summary>

public class TimerItem : PoolManager.IPoolData
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// 用于重置最大计时时间
    /// </summary>
    private int _saveAllTime;

    /// <summary>
    /// 总共计时时间（毫秒）
    /// </summary>
    public int AllTime;

    /// <summary>
    /// 用于重置重复执行时间
    /// </summary>
    public int SaveIntervalTime;

    /// <summary>
    /// 重复执行时间
    /// </summary>
    public int IntervalTime;

    /// <summary>
    /// 计时结束执行回调
    /// </summary>
    public UnityAction OverCallback;

    /// <summary>
    /// 重复执行回调
    /// </summary>
    public UnityAction IntervalCallback;

    public bool IsRuning = true;

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="allTime"></param>
    /// <param name="overCallback"></param>
    /// <param name="intervalTime"></param>
    /// <param name="intervalCallback"></param>
    public void InitInfo(int id, int allTime, UnityAction overCallback, int intervalTime = 0, UnityAction intervalCallback = null)
    {
        this.Id = id;
        this.AllTime = _saveAllTime = allTime;
        this.IntervalTime = SaveIntervalTime = intervalTime;
        this.OverCallback = overCallback;
        this.IntervalCallback = intervalCallback;
    }

    public void RunTimer()
    {
        IsRuning = true;
    }

    public void PauseTimer()
    {
        IsRuning = false;
    }

    /// <summary>
    /// 重置时间
    /// </summary>
    public void ResetTime()
    {
        AllTime = _saveAllTime;
        IntervalTime = SaveIntervalTime;
        IsRuning = true;
    }

    public void ResetInfo()
    {
        IsRuning = false;
        OverCallback = null;
        IntervalCallback = null;
    }
}