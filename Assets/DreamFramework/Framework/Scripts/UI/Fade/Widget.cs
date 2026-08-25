using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class Widget : MonoBehaviour
{
    /// <summary>
    /// 控制Fading速率的动画曲线
    /// 参数：左下角坐标 右上角坐标
    /// </summary>
    [SerializeField] private AnimationCurve _fadingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    /// <summary>
    /// 提供给外部方便查看当前不透明度的属性
    /// </summary>

    public float RenderOpacity => _canvasGroup.alpha;

    private Coroutine _fadeCoroutine;

    private CanvasGroup _canvasGroup;
    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }
    /// <summary>
    /// 过渡
    /// </summary>
    /// <param name="duration">过渡时间为0 则同步瞬间改变透明度</param>
    public void Fade(float opacity, float duration, Action onFinished = null)
    {
        if(duration <= 0)
        {
            _canvasGroup.alpha = opacity;
            onFinished?.Invoke();
            return;
        }
        if(_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = null;
        }

        _fadeCoroutine = StartCoroutine(FadingCoroutine(opacity, duration, onFinished));
    }

    private IEnumerator FadingCoroutine(float opacity, float duration, Action onFinished)
    {
        float timer = 0;
        float startAlpha = _canvasGroup.alpha;
        while(timer < duration)
        {
            //Evaluate 传入x 输出y
            //默认的循环模式(Weap Mode)为Clamp（钳制模式）
            //对于任何大于最后一个关键帧时间，Evaluate() 的结果都会被钳制在最后一个关键帧的值上
            //对于任何小于第一个关键帧时间，Evaluate() 的结果都会被钳制在第一个关键帧的值上
            //另外还有Loop（循环模式）
            //当时间超过曲线范围后，曲线会从头开始重复。就像循环播放的动画一样
            //和PingPong（乒乓模式）
            //当时间超过曲线范围后，曲线会“反向”播放回来。就像乒乓球一样来回反弹
            //在Inspector面板中修改循环模式
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, opacity, _fadingCurve.Evaluate(timer / duration));
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        //最后赋值确保成功
        _canvasGroup.alpha = opacity;
        onFinished?.Invoke();

    }
}
