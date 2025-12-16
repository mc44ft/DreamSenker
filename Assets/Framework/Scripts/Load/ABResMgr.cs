using UnityEngine;
using UnityEngine.Events;

public class ABResMgr : BaseManager<ABResMgr>
{
    /// <summary>
    /// 这个参数用于控制在编辑器模式下是否处于调试状态
    /// 如果处于调试状态 使用编辑器加载
    /// 否则使用AB包加载
    /// </summary>
    private bool _isDebug = true;

    private ABResMgr()
    { }

    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback = null, bool isAsync = true) where T : Object
    {
#if UNITY_EDITOR
        if (_isDebug)
        {
            //所有资源都放置在 编辑器文件夹下的ArtRes文件夹中 资源的文件夹名称就是其正式打包出去的包名
            callback?.Invoke(EditorResMgr.Instance.LoadRes<T>(abName + "/" + resName));
        }
        else
        {
            ABMgr.Instance.LoadResAsync<T>(abName, resName, callback, isAsync);
        }
#else
            ABMgr.Instance.LoadResAsync<T>(abName, resName, callback, isAsync);
#endif
    }
}