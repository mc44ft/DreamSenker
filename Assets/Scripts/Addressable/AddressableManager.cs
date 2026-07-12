using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class AssetInfo
{
    public AsyncOperationHandle Handle;
    private int _count;
    public int Count
    {
        get => _count;
        set => _count = value < 0 ? 0 : value;
    }
    
    public AssetInfo(AsyncOperationHandle handle)
    {
        Handle = handle;
        Count = 0;
    }
}
public class AddressableManager : BaseManager<AddressableManager>
{
    //存引用计数
    private readonly Dictionary<string, AssetInfo> _assetInfoDict =  new();

    //改用 UniTask/Task 走 async/await，try/catch 天然处理异常，根本不用纠结回调签名。
    public async UniTask<T> LoadAssetAsync<T>(string name) where T : UnityEngine.Object
    {
        string key = "name_" + name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        T result;
        if (_assetInfoDict.TryGetValue(key, out AssetInfo info))
        {
            //该资源的引用计数+1
            info.Count++;
            //取出来
            handle = info.Handle.Convert<T>();
            //包一层异常捕获 处理后 向上传递
            try
            {
                result = await handle;
                return result;
            }
            catch
            {
                //回滚Count
                info.Count--;
                throw;
            }
            
        }
        handle = Addressables.LoadAssetAsync<T>(name);
        
        _assetInfoDict[key] = new AssetInfo(handle);
        _assetInfoDict[key].Count++;
        try
        {
            //这里直接await handle 走的是UniTask的拓展
            //await handle.Task 是走的原来的 Task
            result = await handle;
            return result;
        }
        catch
        {
            _assetInfoDict.Remove(key);
            //原样上报
            throw;
        }
    }
    /// <summary>
    /// 加载单个资源
    /// 单资源就用name加载 多资源就用label加载
    /// </summary>
    /// <param name="name"></param>
    /// <param name="completed"></param>
    /// <typeparam name="T"></typeparam>
    public void LoadAssetAsync<T>(string name, Action<T> completed, Action<Exception> failed = null) where T : UnityEngine.Object
    {
        string key = "name_" + name + "_" + typeof(T).Name;
        if (TryGetCache(key, completed, failed))
            return;
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(name);
        
        _assetInfoDict[key] = new AssetInfo(handle);
        _assetInfoDict[key].Count++;
        
        handle.Completed += (operationHandle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                completed?.Invoke(operationHandle.Convert<T>().Result);
            }
            else
            {
                _assetInfoDict.Remove(key);
                //operationHandle.OperationException 是 Addressables 加载失败时挂上的异常对象，但它不保证一定不为 null。失败场景下它可能是 null 的情况：
                // 加载被取消
                // 资源 key 不存在，某些版本不抛异常只标记 Status = Failed
                // 内部错误没构造 Exception 就把状态置为失败
                var ex = operationHandle.OperationException ?? new Exception($"Load {key} failed");
                failed?.Invoke(ex);   // 失败时通知调用方，把异常交出去
            }
        };
    }

    public async UniTask<IList<T>> LoadAssetsAsync<T>(string label, Addressables.MergeMode mode = Addressables.MergeMode.Union) where T : UnityEngine.Object
    {
        string key = "label_" + label + "_" + typeof(T).Name;
        
        AsyncOperationHandle<IList<T>> handle;
        IList<T> result;
        if (_assetInfoDict.TryGetValue(key, out AssetInfo info))
        {
            //该资源的引用计数+1
            info.Count++;
            //取出来
            handle = info.Handle.Convert<IList<T>>();
            //包一层异常捕获 处理后 向上传递
            try
            {
                result = await handle;
                return result;
            }
            catch
            {
                info.Count--;
                throw;
            }
            
        }
        handle = Addressables.LoadAssetsAsync<T>(label, null, mode);
        
        _assetInfoDict[key] = new AssetInfo(handle);
        _assetInfoDict[key].Count++;
        try
        {
            //这里直接await handle 走的是UniTask的拓展
            //await handle.Task 是走的原来的 Task
            result = await handle;
            return result;
        }
        catch
        {
            _assetInfoDict.Remove(key);
            //原样上报
            throw;
        }
    }
    /// <summary>
    /// 用label 加载多个资源
    /// key可以是name 也可以是label
    /// 但更推荐使用label name就当它是唯一的就好了
    /// </summary>
    /// <param name="label"></param>
    /// <param name="completed"></param>
    /// <param name="mergeMode"></param>
    /// <typeparam name="T"></typeparam>
    public void LoadAssetsAsync<T>(string label, Action<IList<T>> completed, Action<Exception> failed = null, 
        Addressables.MergeMode mergeMode = Addressables.MergeMode.Union) where T : UnityEngine.Object
    {
        string key = "label_" + label + "_" + typeof(T).Name;

        if (TryGetCache(key, completed, failed)) return;
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null, mergeMode);
        _assetInfoDict[key] = new AssetInfo(handle);
        _assetInfoDict[key].Count++;
        handle.Completed += (operationHandle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                completed?.Invoke(operationHandle.Result);
            }
            else
            {
                _assetInfoDict.Remove(key);
                var ex = operationHandle.OperationException ?? new Exception($"Load {key} failed");
                failed?.Invoke(ex);   // 失败时通知调用方，把异常交出去
            }
        };
    }
    
    private bool TryGetCache<T>(string key, Action<T> completed, Action<Exception> failed)
    {
        //缓存命中
        if (_assetInfoDict.TryGetValue(key, out AssetInfo info))
        {
            //该资源的引用计数+1
            info.Count++;
            //取出来
            AsyncOperationHandle handle = info.Handle;
            //如果加载完成 直接执行
            if (handle.IsDone)
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    completed?.Invoke(handle.Convert<T>().Result);
                }
                else
                {
                    info.Count--;
                    var ex = handle.OperationException ?? new Exception($"Load {key} failed");
                    failed?.Invoke(ex);
                }
            }
            else//如果还没加载完成 传入回调 等加载完成了再执行
            {
                handle.Completed += (operationHandle) =>
                {
                    if (operationHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        completed?.Invoke(operationHandle.Convert<T>().Result);
                    }
                    else
                    {
                        info.Count--;
                        var ex = operationHandle.OperationException ?? new Exception($"Load {key} failed");
                        failed?.Invoke(ex);
                    }
                };
            }
            return true;
        }

        return false;
    }
    public void ReleaseByName<T>(string name) where T : UnityEngine.Object
    {
        string key = "name_" + name + "_" + typeof(T).Name;
        ReleaseAsset<T>(key);
    }
    public void ReleaseByLabel<T>(string label) where T : UnityEngine.Object
    {
        string key = "label_" + label + "_" + typeof(T).Name;
        ReleaseAsset<T>(key);
    }
    private void ReleaseAsset<T>(string key) where T : UnityEngine.Object
    {
        if (_assetInfoDict.TryGetValue(key,  out AssetInfo info))
        {
            if (info.Count == 0)
            {
                //进这里说明这个字典值是错误存在的，但里面的内容肯定被释放了
                //只Remove就可以
                _assetInfoDict.Remove(key);
                Debug.LogWarning($"Asset {key} already released!");
                return;
            }
            
            info.Count--;
            if (info.Count == 0)
            {
                Addressables.Release(info.Handle);
                _assetInfoDict.Remove(key);
            }
        }
        else
        {
            Debug.LogError($"Asset {key} not found!");
        }
    }
    public void Clear()
    {
        foreach (var info in _assetInfoDict.Values)
        {
            if (info.Count > 0) Debug.LogWarning($"[AddressableManager] Force releasing asset, Count={info.Count}. Someone may still hold a reference.");
            
            var handle = info.Handle;
            Addressables.Release(handle);
        }
        _assetInfoDict.Clear();
    }
}
