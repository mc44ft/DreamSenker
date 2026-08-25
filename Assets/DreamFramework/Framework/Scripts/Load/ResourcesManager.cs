using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    #region 注意

    //1. 加入引用计数的ResMgr
    // 我们在使用资源时就需要有用就有删
    // 当使用某个资源的对象移除时，一定要记得调用移除方法
    //2. 如果觉得卸载资源的功能麻烦，也完全可以不使用卸载的相关方法（没有内存压力的时候可以这么做）
    // 加载相关逻辑不会有任何影响，和以前直接使用Resources的用法几乎一样
    // 只需要再添加一个主动清空字典的方法即可
    //3.异步加载不许传入lambda表达式 因为卸载资源要指明函数

    #endregion 注意

    #region 内部类

    private abstract class ResInfoBase
    {
        //引用计数
        public int RefCount = 0;
    }
    private class ResInfo<T> : ResInfoBase
    {
        public T Asset;
        public UnityAction<T> Callback;
        public Coroutine Coroutine;

        //当引用计数为零时 是否需要立即卸载
        //这个标识的目的是为了防止频繁卸载资源带来的卡顿
        //引用计数已经为零的资源可以等待合适的时机一起卸载
        public bool isDel = false;

        public void AddRefCount()
        {
            RefCount++;
        }

        public void SubRefCount()
        {
            RefCount--;
            if (RefCount < 0)
                Debug.LogError("引用计数小于零！！！");
        }
    }

    #endregion 内部类

    //存储加载过或加载中的资源容器
    private Dictionary<string, ResInfoBase> _resDict = new Dictionary<string, ResInfoBase>();

    private string _key;

    private ResourcesManager() { }



    #region Main Methods

    #region 异步加载
    /// <summary>
    /// 异步加载资源（泛型）
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径（Resources下）</param>
    /// <param name="callback">加载完成时的回调函数</param>
    public void LoadAsync<T>(string path, UnityAction<T> callback = null) where T : UnityEngine.Object
    {
        _key = GetKey(path, typeof(T).Name);
        ResInfo<T> info;
        //第一次请求加载
        if (!_resDict.ContainsKey(_key))
        {
            //声明资源信息对象
            info = new ResInfo<T>();
            info.Callback += callback;
            //将该资源加入到字典中（这时资源还未加载成功）
            _resDict.Add(_key, info);
            //开启协程进行异步加载 并记录协同程序
            info.Coroutine = MonoManager.Instance.StartCoroutine(LoadAsyncCoroutine());
        }
        else
        {
            info = _resDict[_key] as ResInfo<T>;
            //资源已在字典中，但还未加载完
            if (info.Asset == null)
            {
                //将本次委托添加进去 等待加载完成后一起执行
                info.Callback += callback;
            }
            //资源已加载完成
            else
            {
                callback?.Invoke(info.Asset);//直接执行回调函数
            }
        }
        info.AddRefCount();//引用计数加一

        //这种内部函数的写法不会给性能带来提升 甚至会有略微降低（但极其微小，小到可以忽略不计，因为产生闭包时C#会分配额外空间，可能会产生一次微小的GC）
        //但这种写法大大提高了代码的简洁性，使得代码结构更加清晰 这是一种极其提倡的现代化写法
        IEnumerator LoadAsyncCoroutine()
        {
            ResourceRequest rq = Resources.LoadAsync<T>(path);
            yield return rq;
            if (_resDict.ContainsKey(_key))
            {
                info = (_resDict[_key] as ResInfo<T>);
                //资源加载成功后 取出对应的资源信息 加入到字典对象中
                info.Asset = rq.asset as T;
                //加载成功后 如果该资源已经被标记为卸载 就及时卸载
                //为什么会出现异步加载还未加载完成就被卸载的情况呢？
                //因为用户的操作（或游戏逻辑的改变）比资源的加载速度要快。
                //可能上一个资源还未加载完成 用户就迫不及待的切换到了下一个资源
                //这种情况很常见
                if (info.RefCount == 0)
                {
                    UnloadAsset<T>(path, info.isDel, null, false);
                }
                else
                {
                    info.Callback?.Invoke(info.Asset);
                    //清空引用（避免引用占用可能导致的潜在的内存泄漏问题）
                    info.Callback = null;
                    info.Coroutine = null;
                }
            }
        }
    }

    /// <summary>
    /// 异步加载资源（类型）
    /// 这个方法和泛型方法不要混用，不要用来加载同一个资源
    /// 如果该方法和泛型方法同时用来加载同一个资源 因为其键相同但值不同，可能会出现报错
    /// 这里将该方法设置为过时方法 最好不使用这个方法
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="type">资源类型</param>
    /// <param name="callback">加载完成时的回调函数</param>
    [Obsolete("最好不要使用！！！如果要用，不要和泛型方法混用")]
    public void LoadAsync(string path, Type type, UnityAction<UnityEngine.Object> callback = null)
    {
        _key = GetKey(path, type.Name);
        ResInfo<UnityEngine.Object> info;
        if (!_resDict.ContainsKey(_key))
        {
            //声明资源信息对象
            info = new ResInfo<UnityEngine.Object>();
            info.Callback += callback;
            //将该资源加入到字典中（这时资源还未加载成功）
            _resDict.Add(_key, info);
            //开启协程进行异步加载 并记录协同程序
            info.Coroutine = MonoManager.Instance.StartCoroutine(LoadAsyncCoroutine());
        }
        else
        {
            info = _resDict[_key] as ResInfo<UnityEngine.Object>;
            //资源还未加载完成
            if (info.Asset == null)
            {
                //将本次委托添加进去 等待加载完成后一起执行
                info.Callback += callback;
            }
            else
            {
                callback?.Invoke(info.Asset);//直接执行回调函数
            }
        }
        info.AddRefCount();//引用计数加一

        IEnumerator LoadAsyncCoroutine()
        {
            //这里使用传入具体类型的重载 如果有不同类型但是同名的多个资源 可以精准的找到指定资源
            ResourceRequest rq = Resources.LoadAsync(path, type);
            yield return rq;
            if (_resDict.ContainsKey(_key))
            {
                info = (_resDict[_key] as ResInfo<UnityEngine.Object>);
                //资源加载成功后 取出对应的资源信息 加入到字典对象中
                info.Asset = rq.asset;
                if (info.RefCount == 0)
                {
                    UnloadAsset(path, type, info.isDel, null, false);
                }
                else
                {
                    info.Callback?.Invoke(info.Asset);
                    //清空引用（避免引用占用可能导致的潜在的内存泄漏问题）
                    info.Callback = null;
                    info.Coroutine = null;
                }
            }
        }
    }

    #endregion 异步加载

    #region 同步加载

    /// <summary>
    /// 同步加载资源（泛型）
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    public T Load<T>(string path) where T : UnityEngine.Object
    {
        //当异步加载还没加载结束时 同步加载了 怎么处理
        string key = GetKey(path, typeof(T).Name);
        ResInfo<T> info;
        //第一次加载
        if (!_resDict.ContainsKey(key))
        {
            info = new ResInfo<T>();
            T asset = Resources.Load<T>(path);
            info.Asset = asset;

            _resDict.Add(key, info);
        }
        else
        {
            info = _resDict[key] as ResInfo<T>;
            //第一次加载是异步加载 且还未加载结束
            if (info.Asset == null)
            {
                //同步加载可以打断异步加载
                //停止异步加载
                MonoManager.Instance.StopCoroutine(info.Coroutine);
                T asset = Resources.Load<T>(path);
                info.Asset = asset;
                info.Callback?.Invoke(info.Asset);
                info.Callback = null;
                info.Coroutine = null;
            }
        }
        info.AddRefCount();//引用计数加一
        return info.Asset;
    }

    /// <summary>
    /// 同步加载资源（类型）
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    [Obsolete("最好不要使用！！！如果要用，不要和泛型方法混用")]
    public UnityEngine.Object Load(string path, Type type)
    {
        string key = GetKey(path, type.Name);
        ResInfo<UnityEngine.Object> info;
        if (!_resDict.ContainsKey(key))
        {
            info = new ResInfo<UnityEngine.Object>();
            UnityEngine.Object asset = Resources.Load(path, type);
            info.Asset = asset;
            _resDict.Add(key, info);
        }
        else
        {
            info = _resDict[key] as ResInfo<UnityEngine.Object>;
            //资源还没加载结束
            if (info.Asset == null)
            {
                //停止异步加载
                MonoManager.Instance.StopCoroutine(info.Coroutine);
                UnityEngine.Object asset = Resources.Load(path, type);
                info.Asset = asset;
                info.Callback?.Invoke(info.Asset);
                info.Callback = null;
                info.Coroutine = null;
            }
        }
        info.AddRefCount();//引用计数加一
        return info.Asset;
    }

    #endregion 同步加载

    #region 卸载

    //注意：卸载资源是立即执行的（大部分资源立即从内存或显存中被释放 而不是等待下一次GC）

    /// <summary>
    /// 指定同步卸载一个资源（泛型）
    /// </summary>
    /// <typeparam name="T">要卸载的资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <param name="isDel">是否立即删除</param>
    /// <param name="callback">要取消的回调函数</param>
    /// <param name="isSub">引用计数已经为零时传入可以不去减引用计数</param>
    public void UnloadAsset<T>(string path, bool isDel = false, UnityAction<T> callback = null, bool isSub = true) where T : UnityEngine.Object
    {
        string key = GetKey(path, typeof(T).Name);

        if (_resDict.ContainsKey(key))
        {
            ResInfo<T> info = _resDict[key] as ResInfo<T>;
            if (isSub)
                info.SubRefCount();//引用计数减1
            info.isDel = isDel;
            if (info.Asset != null && info.RefCount == 0 && info.isDel)
            {
                _resDict.Remove(key);
                Resources.UnloadAsset(info.Asset as UnityEngine.Object);
            }
            //资源正在异步加载中（也就是说资源还未加载成功，且所有的回调函数都没执行
            else if (info.Asset == null)
            {
                //如果资源正在异步加载中 移除该次委托（还未执行的委托）
                //在正在异步加载的协程函数中 判断引用是否为0再继续卸载 并且传入本次传入的isDel值
                if (callback != null)
                    info.Callback -= callback;
            }
            //else情况 只进行引用次数的减一
        }
    }

    /// <summary>
    /// 指定同步卸载一个资源（类型）
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="type">资源类型</param>
    public void UnloadAsset(string path, Type type, bool isDel = false, UnityAction<UnityEngine.Object> completedCallback = null, bool isSub = true)
    {
        string key = GetKey(path, type.Name);
        if (_resDict.ContainsKey(key))
        {
            ResInfo<UnityEngine.Object> info = _resDict[key] as ResInfo<UnityEngine.Object>;
            if (isSub)
                info.SubRefCount();//引用计数减1
            info.isDel = isDel;
            if (info.Asset != null && info.RefCount == 0 && info.isDel)
            {
                _resDict.Remove(key);
                Resources.UnloadAsset(info.Asset);
            }
            //资源正在异步加载中
            else if (info.Asset == null)
            {
                if (completedCallback != null)
                    info.Callback -= completedCallback;
            }
        }
    }

    /// <summary>
    /// 异步卸载所有无引用的资源（可以在过场景或某些特殊时刻调用该方法来卸载掉所有无引用的对象）
    /// </summary>
    public void UnloadUnusedAssets(UnityAction callback = null)
    {
        MonoManager.Instance.StartCoroutine(UnloadUnusedAssetsCoroutine());
        IEnumerator UnloadUnusedAssetsCoroutine()
        {
            List<string> list = new List<string>();
            foreach (string key in _resDict.Keys)
            {
                if (_resDict[key].RefCount == 0)
                    list.Add(key);//记录要移除的字典键
            }
            foreach (string key in list)
            {
                _resDict.Remove(key);//移除记录的字典键值对
            }
            AsyncOperation ao = Resources.UnloadUnusedAssets();
            yield return ao;
            callback?.Invoke();
        }
    }

    /// <summary>
    /// 不想配套用卸载时可以用这个方法 简单些
    /// </summary>
    /// <param name="callback"></param>
    public void ClearDic(UnityAction callback = null)
    {
        MonoManager.Instance.StartCoroutine(UnloadUnusedAssetsCoroutine());
        IEnumerator UnloadUnusedAssetsCoroutine()
        {
            _resDict.Clear();
            AsyncOperation ao = Resources.UnloadUnusedAssets();
            yield return ao;
            callback?.Invoke();
        }
    }

    #endregion 卸载

    #endregion Main Methods
    #region Other Methods

    private string GetKey(string path, string typeName)
    {
        return path + "_" + typeName;
    }

    public int GetRefCount<T>(string path) where T : UnityEngine.Object
    {
        string key = GetKey(path, typeof(T).Name);
        if (_resDict.ContainsKey(key))
        {
            return (_resDict[key] as ResInfo<T>).RefCount;
        }
        else
        {
            return 0;
        }
    }

    #endregion Other Methods
}