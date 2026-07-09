using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
//改用 UniTask/Task 走 async/await，try/catch 天然处理异常，根本不用纠结回调签名。
public class AssetInfo
{
    public AsyncOperationHandle Handle;
    private int _count;
    public int Count
    {
        get
        {
            return _count;
        }
        set
        {
            _count = value < 0 ? 0 : value;
        }
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
    
    /// <summary>
    /// 加载单个资源
    /// </summary>
    /// <param name="name"></param>
    /// <param name="completed"></param>
    /// <typeparam name="T"></typeparam>
    public void LoadAssetAsync<T>(string name, Action<T> completed = null) where T : UnityEngine.Object
    {
        string key = name + "_" + typeof(T).Name;
        if (TryGetCache(key, completed))
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
                Debug.LogError($"Asset {key} not Loaded!");
            }
        };
    }

    /// <summary>
    /// 加载单个资源
    /// 单资源就用name加载 多资源就用label加载 这个函数之后删掉
    /// </summary>
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="completed"></param>
    /// <typeparam name="T"></typeparam>
    public void LoadAssetAsync<T>(string name, string label, Action<T> completed = null) where T : UnityEngine.Object
    {
        string key = name + "_" + label + "_" + typeof(T).Name;

        if (TryGetCache(key, completed))
            return;
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<T>(new List<string>(){name, label});
        
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
                Debug.LogError($"Asset {key} not Loaded!");
            }
        };
    }

    private bool TryGetCache<T>(string key, Action<T> completed)
    {
        //缓存命中
        if (_assetInfoDict.TryGetValue(key, out AssetInfo info))
        {
            //该资源的引用计数+1
            _assetInfoDict[key].Count++;
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
                    _assetInfoDict.Remove(key);
                    Debug.LogError($"Asset {key} not Loaded!");
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
                        _assetInfoDict.Remove(key);
                        Debug.LogError($"Asset {key} not Loaded!");
                    }
                };
            }
            return true;
        }

        return false;
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
    public void LoadAssetsAsync<T>(string label, Action<IList<T>> completed, Addressables.MergeMode mergeMode = Addressables.MergeMode.Union) where T : UnityEngine.Object
    {
        string key = label + "_" + typeof(T).Name;

        if (TryGetCache(key, completed)) return;
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
                Debug.LogError($"Asset {key} not Loaded!");
            }
        };
        
    }
    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="names"></param>
    // /// <param name="completed"></param>
    // /// <param name="mergeMode"></param>
    // public void LoadAssetsAsync<T>(List<string> names, Action<T> completed = null, 
    //     Addressables.MergeMode mergeMode = Addressables.MergeMode.Union) where T : UnityEngine.Object
    // {
    //     if (names == null || names.Count == 0) return;
    //     
    //     //拼接key
    //     string key = "";
    //     foreach (var name in names)
    //     {
    //         key += name + "_";
    //     }
    //     key += typeof(T).Name;
    //     
    //     if (TryGetCache(key, completed)) return;
    //     
    //     AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(names, completed, mergeMode);
    //     _assetInfoDict[handle]++;
    // }
    public void Release<T>(string name) where T : UnityEngine.Object
    {
        string key = name + "_" + typeof(T).Name;
        ReleaseAsset<T>(key);
    }

    public void Release<T>(string name, string label) where T : UnityEngine.Object
    {
        string key = name + "_" + label + "_" + typeof(T).Name;
        ReleaseAsset<T>(key);
    }
    private void ReleaseAsset<T>(string key) where T : UnityEngine.Object
    {
        if (_assetInfoDict.ContainsKey(key))
        {
            _assetInfoDict[key].Count--;
            if (_assetInfoDict[key].Count == 0)
            {
                AsyncOperationHandle handle = _assetInfoDict[key].Handle;
                Addressables.Release(handle);
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
            var handle = info.Handle;
            Addressables.Release(handle);
        }
        _assetInfoDict.Clear();
    }
}
