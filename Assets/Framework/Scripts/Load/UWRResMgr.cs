using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// 因为安卓平台无法通过C#自带的File类来访问StreamingAssets文件夹
/// 所以用UnityWebRequest来加载资源
/// </summary>

public class UWRResMgr : SingletonAutoMono<UWRResMgr>
{
    /// <summary>
    /// 加载资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径 需要填写后缀
    /// 需完整协议：
    /// http://
    /// ftp://
    /// file://</param>
    /// <param name="successCallback">加载成功回调函数</param>
    /// <param name="failCallback">加载失败回调函数</param>
    public void LoadRes<T>(string path, UnityAction<T> successCallback = null, UnityAction failCallback = null) where T : class
    {
        StartCoroutine(LoadResCoroutine());
        IEnumerator LoadResCoroutine()
        {
            Type type = typeof(T);
            UnityWebRequest req = null;
            if (type == typeof(string) || type == typeof(byte[]))
            {
                req = UnityWebRequest.Get(path);
            }
            else if (type == typeof(Texture))
            {
                req = UnityWebRequestTexture.GetTexture(path);
            }
            else if (type == typeof(AssetBundle))
            {
                req = UnityWebRequestAssetBundle.GetAssetBundle(path);
            }
            else
            {
                failCallback?.Invoke();
                yield break;//停止协程
            }
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                if (type == typeof(string))
                {
                    successCallback?.Invoke(req.downloadHandler.text as T);
                }
                else if (type == typeof(byte[]))
                {
                    successCallback?.Invoke(req.downloadHandler.data as T);
                }
                else if (type == typeof(Texture))
                {
                    successCallback?.Invoke(DownloadHandlerTexture.GetContent(req) as T);
                }
                else if (type == typeof(AssetBundle))
                {
                    successCallback?.Invoke(DownloadHandlerAssetBundle.GetContent(req) as T);
                }
            }
            else
            {
                failCallback.Invoke();
            }
            req.Dispose();//释放UnityWebRequest对象
        }
    }
}