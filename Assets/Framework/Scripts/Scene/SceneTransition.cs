using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 场景切换过渡 需要用的时候将该预制体放在场景上即可
/// 需要Loading图片，直接替换即可
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SceneTransition : SingletonMono<SceneTransition>
{
    [SerializeField] private Image _loadingImage;
    [Tooltip("过渡持续时间（黑屏）")]
    [SerializeField] private float _fadeDurationBlack = 0.5f;
    [Tooltip("过渡持续时间（亮屏）")]
    [SerializeField] private float _fadeDurationLight = 0.5f;
    [Tooltip("场景加载完成后的等待时间（不受缩放时间影响） 等待时间结束后将显示新场景")]
    [SerializeField] private float _waitRealTime = 0f;
    /// <summary>
    /// 场景加载完成后，要执行的回调
    /// </summary>
    private Action _onFinished;
    /// <summary>
    /// 场景黑屏后 尚未加载下一个场景前，要执行的回调
    /// </summary>
    private Action _onFadeBlack;

    /// <summary>
    /// 使用该组件调节场景过渡背景的透明度
    /// </summary>
    private CanvasGroup _fadeCanvasGroup;
    protected override void Awake()
    {
        base.Awake();
        _fadeCanvasGroup = GetComponent<CanvasGroup>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="onFadeBlack">当前场景黑屏后 还未加载下一场景前执行</param>
    /// <param name="onFinished">场景加载结束 尚未亮屏时执行</param>
    public void LoadScene(string sceneName, Action onFadeBlack = null, Action onFinished = null)
    {
        _onFadeBlack = onFadeBlack;
        _onFinished = onFinished;
        
        StartCoroutine(SceneTransitionCoroutine(sceneName));
    }
    public void ResetLoading(Sprite sprite, float waitRealTime)
    {
        _loadingImage.sprite = sprite;
        _waitRealTime = waitRealTime;
    }
    private IEnumerator SceneTransitionCoroutine(string sceneName)
    {
        yield return Fade(1f, _fadeDurationBlack);

        _onFadeBlack?.Invoke();
        _onFadeBlack = null;

        AsyncOperation operation = MySceneManager.Instance.LoadSceneAsyncOperation(sceneName);
        //先不激活场景 等效果播放完毕再激活场景
        //此行会导致场景加载到0.9就暂停加载
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            //加载进度达到0.9 就说明核心资源都加载完毕
            if (operation.progress >= 0.9f)
            {
                //允许激活新场景，此时会自动完成最后10%的加载并切换
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        _onFinished?.Invoke();
        _onFinished = null;

        yield return new WaitForSecondsRealtime(_waitRealTime);

        yield return Fade(0f, _fadeDurationLight);
    }
    private IEnumerator Fade(float targetAlpha, float fadeDuration)
    {
        //开始淡入淡出时 禁止鼠标点击
        _fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = _fadeCanvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            //线性插值
            _fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }
        _fadeCanvasGroup.alpha = targetAlpha;

        //淡入完成（场景切换结束） 恢复鼠标点击
        if (targetAlpha == 0)
        {
            _fadeCanvasGroup.blocksRaycasts = false;
        }
    }
}
