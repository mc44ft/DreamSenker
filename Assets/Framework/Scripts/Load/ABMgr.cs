using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AB包管理器
/// 让外部更方便的进行资源加载
/// </summary>
public class ABMgr : SingletonAutoMono<ABMgr>
{
    //主包（一个平台对应一个主包，每个平台都有不同的主包）
    private AssetBundle _mainAB;
    //主包中的固定文件（AssetBundleManifest） 用于获取依赖信息
    private AssetBundleManifest _abManifest;
    //AB包不能重复加载
    private readonly Dictionary<string, AssetBundle> _abDict = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// AB包存放路径
    /// </summary>
    private readonly string _pathUrl = Application.streamingAssetsPath + "/";
    /// <summary>
    /// 主包名称
    /// </summary>
    private string MainAB_Name
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_STANDALONE_WIN
            return "Windows";
#else 
            return "PC";
#endif
        }
    }

    #region 包加载

    private void LoadMianAB()
    {
        //加载主包
        if (_mainAB == null)
        {
            _mainAB = AssetBundle.LoadFromFile(_pathUrl + MainAB_Name);
            //获取其依赖包
            _abManifest = _mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }

    /// <summary>
    /// 加载主包、AB包及其依赖包
    /// </summary>
    /// <param name="abName"></param>
    public void LoadDependencies(string abName)
    {
        LoadMianAB();
        //获取依赖包信息
        string[] strs = _abManifest.GetAllDependencies(abName);
        AssetBundle ab;
        for (int i = 0; i < strs.Length; i++)
        {
            if (!_abDict.ContainsKey(strs[i]))
            {
                //加载依赖包
                ab = AssetBundle.LoadFromFile(_pathUrl + strs[i]);
                if (ab != null)
                    _abDict.Add(strs[i], ab);
                else
                    Debug.LogError("加载AB包失败: " + strs[i]);
            }
        }
        //加载AB包
        if (!_abDict.ContainsKey(abName))
        {
            ab = AssetBundle.LoadFromFile(_pathUrl + abName);
            _abDict.Add(abName, ab);
        }
    }

    #endregion 包加载

    #region 同步加载

    //这里取消了同步加载函数 同步加载整合到异步加载函数中

    #endregion 同步加载

    #region 异步加载

    //异步加载AB包的问题
    //AB包重复加载会报错
    //在异步加载和同步加载冲突的情况下 即使在同步加载前停止了异步加载的协程，依然会报错
    //就是说不论AB包有没有加载成功 只要加载了一次 再次加载（没有卸载）一定会报错
    //所以即便是同步加载 只要是重复加载 就只能等待第一次异步加载成功再使用
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="abName"></param>
    /// <param name="resName">资源名（无需后缀）</param>
    /// <param name="callback"></param>
    /// <param name="isAsync">是否启用异步加载，不启用为同步</param>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback = null, bool isAsync = true) where T : Object
    {
        StartCoroutine(LoadResAsyncCoroutine());
        IEnumerator LoadResAsyncCoroutine()
        {
            //主包保留同步加载
            LoadMianAB();
            //获取依赖包信息
            string[] strs = _abManifest.GetAllDependencies(abName);
            for (int i = 0; i < strs.Length; i++)
            {
                if (!_abDict.ContainsKey(strs[i]))
                {
                    if (isAsync)//异步加载
                    {
                        //这里用空值来占位
                        //目的是告诉外部这个包已经开始加载了
                        //避免重复加载同一个包
                        _abDict.Add(strs[i], null);
                        //加载依赖包
                        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_pathUrl + strs[i]);
                        yield return req;
                        if (req.assetBundle != null)
                            _abDict[strs[i]] = req.assetBundle;
                        else
                            Debug.LogError("加载AB包失败: " + strs[i]);
                    }
                    else//同步加载
                    {
                        _abDict.Add(strs[i], AssetBundle.LoadFromFile(_pathUrl + strs[i]));
                    }
                }
                else
                {
                    //如果此时字典中的值为null 说明正在加载该包 需要等待其加载完成再执行后续逻辑
                    while (_abDict[strs[i]] == null)
                    {
                        //不论同步还是异步 这里都需要等待
                        //等待该包加载完成
                        yield return null;//等待一帧
                    }
                    //如果不为null 直接执行后续逻辑
                }
            }
            //加载AB包（目标包）
            if (!_abDict.ContainsKey(abName))
            {
                if (isAsync)//异步加载
                {
                    _abDict.Add(abName, null);//占位
                    AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_pathUrl + abName);
                    yield return req;

                    if (req.assetBundle != null)
                        _abDict[abName] = req.assetBundle;
                    else
                        Debug.LogError("加载AB包失败: " + abName);
                }
                else//同步加载
                {
                    _abDict.Add(abName, AssetBundle.LoadFromFile(_pathUrl + abName));//占位
                }
            }
            else
            {
                while (_abDict[abName] == null)
                {
                    //等待该包加载完成
                    yield return null;//等待一帧
                }
            }
            //加载资源
            if (isAsync)
            {
                AssetBundleRequest abr = _abDict[abName].LoadAssetAsync<T>(resName);
                yield return abr;
                callback?.Invoke(abr.asset as T);
            }
            else
            {
                callback?.Invoke(_abDict[abName].LoadAsset<T>(resName));
            }
        }
    }

    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback = null, bool isAsync = true)
    {
        StartCoroutine(LoadResAsyncCoroutine());
        IEnumerator LoadResAsyncCoroutine()
        {
            //加载主包（保留同步加载）
            LoadMianAB();

            //获取依赖包信息
            string[] strs = _abManifest.GetAllDependencies(abName);
            //AssetBundle ab;
            for (int i = 0; i < strs.Length; i++)
            {
                if (!_abDict.ContainsKey(strs[i]))
                {
                    if (isAsync)
                    {
                        //这里用空值来占位
                        //目的是告诉外部这个包已经开始加载了
                        //避免重复加载同一个包
                        _abDict.Add(strs[i], null);
                        //加载依赖包
                        AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_pathUrl + strs[i]);
                        yield return req;
                        if (req.assetBundle != null)
                            _abDict[strs[i]] = req.assetBundle;
                        else
                            Debug.LogError("加载AB包失败: " + strs[i]);
                    }
                    else
                    {
                        _abDict.Add(strs[i], AssetBundle.LoadFromFile(_pathUrl + strs[i]));
                    }
                }
                else
                {
                    //如果此时字典中的值为null 说明正在加载该包 需要等待其加载完成再执行后续逻辑
                    while (_abDict[strs[i]] == null)
                    {
                        //等待该包加载完成
                        yield return null;//等待一帧
                    }
                    //如果不为null 直接执行后续逻辑
                }
            }
            //加载AB包（目标包）
            if (!_abDict.ContainsKey(abName))
            {
                if (isAsync)//异步加载
                {
                    _abDict.Add(abName, null);//占位
                    AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_pathUrl + abName);
                    yield return req;

                    if (req.assetBundle != null)
                        _abDict[abName] = req.assetBundle;
                    else
                        Debug.LogError("加载AB包失败: " + abName);
                }
                else//同步加载
                {
                    _abDict.Add(abName, AssetBundle.LoadFromFile(_pathUrl + abName));//占位
                }
            }
            else
            {
                while (_abDict[abName] == null)
                {
                    //等待该包加载完成
                    yield return null;//等待一帧
                }
            }
            //加载资源
            if (isAsync)
            {
                AssetBundleRequest abr = _abDict[abName].LoadAssetAsync(resName, type);
                yield return abr;
                callback?.Invoke(abr.asset);
            }
            else
            {
                callback?.Invoke(_abDict[abName].LoadAsset(resName, type));
            }
        }
    }

    #endregion 异步加载

    #region 卸载

    /// <summary>
    /// 单个包卸载
    /// </summary>
    /// <param name="abName"></param>
    public void Unload(string abName, bool isUnloadScene = false, UnityAction<bool> callback = null)
    {
        if (_abDict.ContainsKey(abName))
        {
            if (_abDict[abName] == null)
            {
                Debug.LogWarning(abName + "包正在加载中，无法卸载！");
                callback?.Invoke(false);
                return;
            }
            _abDict[abName].Unload(isUnloadScene); // false表示不卸载场景上用AB包加载的资源
            _abDict.Remove(abName);
            callback?.Invoke(true);
        }
        else
        {
            Debug.LogWarning("AB包不存在: " + abName);
        }
    }

    /// <summary>
    /// 所有包卸载
    /// </summary>
    public void UnloadAll(bool isUnloadScene = false)
    {
        StopAllCoroutines();//停止所有协同程序
        AssetBundle.UnloadAllAssetBundles(isUnloadScene);
        _abDict.Clear();
        _mainAB = null;
        _abManifest = null;
    }

    #endregion 卸载
}