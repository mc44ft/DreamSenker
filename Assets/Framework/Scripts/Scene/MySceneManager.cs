using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MySceneManager : BaseManager<MySceneManager>
{
    private MySceneManager()
    { }

    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="mode"></param>
    public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        //加载场景
        SceneManager.LoadScene(sceneName, mode);
    }

    /// <summary>
    /// 异步加载场景（直接版）
    /// 调用者只需关注加载完成后的逻辑即可
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="mode"></param>
    /// <param name="callback"></param>
    public void LoadSceneAsync(string sceneName, UnityAction callback, LoadSceneMode mode = LoadSceneMode.Single)
    {
        MonoManager.Instance.StartCoroutine(LoadSceneAsyncCoroutine());
        IEnumerator LoadSceneAsyncCoroutine()
        {
            //调用灵活版方法
            AsyncOperation operation = LoadSceneAsyncOperation(sceneName, mode);
            //等待场景加载结束
            yield return operation;
            //执行回调方法
            callback?.Invoke();
        }
    }
    /// <summary>
    /// 异步加载场景（灵活版）
    /// 将AsyncOperation交给外部进行更灵活的处理
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public AsyncOperation LoadSceneAsyncOperation(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

        MonoManager.Instance.StartCoroutine(LoadProgressCoroutine(operation));

        return operation;
    }
    /// <summary>
    /// 该协程用于独立广播加载进度
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    private IEnumerator LoadProgressCoroutine(AsyncOperation operation)
    {
        while (!operation.isDone)
        {
            EventCenter.Instance.EventTrigger(E_EventType.SceneLoadProgress, this, new SceneEventArgs(operation.progress));
            yield return null;
        }
        EventCenter.Instance.EventTrigger(E_EventType.SceneLoadOver, this, new EmptyEventArgs());
    }
}