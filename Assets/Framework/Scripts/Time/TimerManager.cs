using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerManager : BaseManager<TimerManager>
{
    /// <summary>
    /// 受到TimeScale影响的计时器项字典
    /// </summary>
    private Dictionary<int, TimerItem> _timerItemDict_Scale = new Dictionary<int, TimerItem>();

    private List<int> _delList_Scale = new List<int>();
    private Coroutine _timerCoroutine_Scale;
    private WaitForSeconds _waitForSeconds = new WaitForSeconds(_intervalTime);//节约性能 不用每次new

    /// <summary>
    /// 不受TimeScale影响的计时器项字典
    /// </summary>
    private Dictionary<int, TimerItem> _timerItemDict_Real = new Dictionary<int, TimerItem>();

    private List<int> _delList_Real = new List<int>();
    private Coroutine _timerCoroutine_Real;
    private WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(_intervalTime);//节约性能 不用每次new

    /// <summary>
    /// 唯一ID
    /// </summary>
    private int TIMER_ID = 0;

    /// <summary>
    /// 监测间隔
    /// </summary>
    private const float _intervalTime = 0.1f; //每100毫秒检查一次

    private TimerManager()
    {
        OpenTimerMgr();//默认计时器就是开启状态
    }

    public void OpenTimerMgr()
    {
        _timerCoroutine_Scale = MonoManager.Instance.StartCoroutine(TimerCoroutine(true, _timerItemDict_Scale));
        _timerCoroutine_Real = MonoManager.Instance.StartCoroutine(TimerCoroutine(false, _timerItemDict_Real));
    }

    public void CloseTimerMgr()
    {
        MonoManager.Instance.StopCoroutine(_timerCoroutine_Scale);
        MonoManager.Instance.StopCoroutine(_timerCoroutine_Real);
    }

    private IEnumerator TimerCoroutine(bool isScale, Dictionary<int, TimerItem> dict)
    {
        while (true)
        {
            if (isScale)
                yield return _waitForSeconds;//每一百毫秒监听一次
            else
                yield return _waitForSecondsRealtime;//每一百毫秒监听一次
            foreach (TimerItem item in dict.Values)
            {
                if (!item.IsRuning)
                {
                    continue;
                }
                //执行重复计时
                if (item.IntervalCallback != null)
                {
                    item.IntervalTime -= (int)(_intervalTime * 1000);
                    if (item.IntervalTime <= 0)
                    {
                        item.IntervalCallback.Invoke();
                        item.IntervalTime = item.SaveIntervalTime; //重置重复计时
                    }
                }
                //不执行重复计时的执行重复计时的都需要执行以下操作
                item.AllTime -= (int)(_intervalTime * 1000);
                if (item.AllTime <= 0)
                {
                    item.OverCallback?.Invoke();
                    _delList_Scale.Add(item.Id); //将需要删除的计时器项ID添加到删除列表
                }
            }
            //清空待移除列表
            for (int i = 0; i < _delList_Scale.Count; i++)
            {
                int id = _delList_Scale[i];
                if (_timerItemDict_Scale.ContainsKey(id))
                {
                    _timerItemDict_Scale[id].ResetInfo(); //重置计时器项信息
                    PoolManager.Instance.PushData(_timerItemDict_Scale[id]); //将计时器项放回对象池
                    _timerItemDict_Scale.Remove(id); //从字典中移除计时器项
                }
            }
            _delList_Scale.Clear();
        }
    }

    /// <summary>
    /// 添加单个计时器（受Scale影响）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="allTime"></param>
    /// <param name="overCallback"></param>
    /// <param name="intervalTime"></param>
    /// <param name="intervalCallback"></param>
    public int AddTimerItem_Scale(int allTime, UnityAction overCallback, int intervalTime = 0, UnityAction intervalCallback = null)
    {
        int id = TIMER_ID++;
        TimerItem item = PoolManager.Instance.PullData<TimerItem>();
        item.InitInfo(id, allTime, overCallback, intervalTime, intervalCallback);
        _timerItemDict_Scale.Add(id, item);
        return id;
    }

    /// <summary>
    /// 添加单个计时器（不受Scale影响）
    /// </summary>
    /// <param name="id"></param>
    /// <param name="allTime"></param>
    /// <param name="overCallback"></param>
    /// <param name="intervalTime"></param>
    /// <param name="intervalCallback"></param>
    public int AddTimerItem_Real(int allTime, UnityAction overCallback, int intervalTime = 0, UnityAction intervalCallback = null)
    {
        int id = TIMER_ID++;
        TimerItem item = PoolManager.Instance.PullData<TimerItem>();
        item.InitInfo(id, allTime, overCallback, intervalTime, intervalCallback);
        _timerItemDict_Real.Add(id, item);
        return id;
    }

    /// <summary>
    /// 移除单个计时器
    /// </summary>
    /// <param name="id"></param>
    public void RemoveTimerItem(int id)
    {
        if (_timerItemDict_Scale.ContainsKey(id))
        {
            _timerItemDict_Scale[id].ResetInfo();
            PoolManager.Instance.PushData(_timerItemDict_Scale[id]);
            _timerItemDict_Scale.Remove(id);
        }
        else if (_timerItemDict_Real.ContainsKey(id))
        {
            _timerItemDict_Real[id].ResetInfo();
            PoolManager.Instance.PushData(_timerItemDict_Real[id]);
            _timerItemDict_Real.Remove(id);
        }
        else
        {
            Debug.LogError("不存在ID为" + id + "的计时器项");
        }
    }

    /// <summary>
    /// 重置单个计时器项信息
    /// </summary>
    /// <param name="id"></param>
    public void ResetTimerItem(int id)
    {
        if (_timerItemDict_Scale.ContainsKey(id))
        {
            _timerItemDict_Scale[id].ResetTime();
        }
        else if (_timerItemDict_Real.ContainsKey(id))
        {
            _timerItemDict_Real[id].ResetTime();
        }
        else
        {
            Debug.LogError("不存在ID为" + id + "的计时器项");
        }
    }

    /// <summary>
    /// 开始单个计时器
    /// </summary>
    /// <param name="id"></param>
    public void StartTimerItem(int id)
    {
        if (_timerItemDict_Scale.ContainsKey(id))
        {
            _timerItemDict_Scale[id].RunTimer();
        }
        else if (_timerItemDict_Real.ContainsKey(id))
        {
            _timerItemDict_Real[id].RunTimer();
        }
        else
        {
            Debug.LogError("不存在ID为" + id + "的计时器项");
        }
    }

    /// <summary>
    /// 暂停单个计时器
    /// </summary>
    /// <param name="id"></param>
    public void StopTimerItem(int id)
    {
        if (_timerItemDict_Scale.ContainsKey(id))
        {
            _timerItemDict_Scale[id].PauseTimer();
        }
        else if (_timerItemDict_Real.ContainsKey(id))
        {
            _timerItemDict_Real[id].PauseTimer();
        }
        else
        {
            Debug.LogError("不存在ID为" + id + "的计时器项");
        }
    }
    #region 计时器

    #endregion
    #region 静态工具
    /// <summary>
    /// 倒计时 可以在外部停止协程来取消执行回调
    /// 启用协程和停止同一个协程 应该由同一个对象来执行
    /// 所以外部也需要用MonoManager来停止协程
    /// </summary>
    /// <param name="time"></param>
    /// <param name="OnFinished"></param>
    /// <param name="isScaled">是否使用Scaled时间</param>
    /// <returns></returns>
    public static Coroutine Countdown(float time, Action OnFinished, bool isScaled = true)
    {
        return MonoManager.Instance.StartCoroutine(CountdownCoroutine());
        IEnumerator CountdownCoroutine()
        {
            float timer = time;
            while (timer > 0)
            {
                timer -= isScaled ? Time.deltaTime : Time.unscaledDeltaTime;
                yield return null;
            }
            OnFinished();
        }
    }
    #endregion

}