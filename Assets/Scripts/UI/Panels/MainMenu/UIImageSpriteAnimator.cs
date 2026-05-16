using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI.Panels.MainMenu
{
public class UIImageSpriteAnimator : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image _targetImage;//用于播放序列帧的 UI Image

    [Header("Frames")]
    [SerializeField] private Sprite[] _defaultFrames;//默认播放序列帧

    [Header("Playback")]
    [SerializeField] private float _frameRate = 8f;//默认播放帧率
    [SerializeField] private bool _loop = true;//是否循环播放默认序列帧
    [SerializeField] private bool _playOnEnable;//启用组件时是否自动播放默认序列帧
    [SerializeField] private bool _useRealtime = true;//是否忽略 Time.timeScale
    [SerializeField] private bool _hideImageWhenStopped;//停止播放时是否隐藏 Image
    [SerializeField] private bool _setNativeSizeEachFrame;//是否每帧按 Sprite 原始尺寸设置 Image 尺寸

    private Coroutine _playRoutine;//当前播放协程
    private bool _isPlaying;//当前是否正在播放

    public bool IsPlaying => _isPlaying;
    public Image TargetImage => _targetImage;

    /// <summary>
    /// 组件启用时按配置播放默认帧动画。
    /// </summary>
    private void OnEnable()
    {
        if (_playOnEnable)
        {
            PlayDefault();
        }
    }

    /// <summary>
    /// 组件禁用时停止当前帧动画。
    /// </summary>
    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// 播放 Inspector 配置的默认序列帧。
    /// </summary>
    public void PlayDefault(Action onComplete = null)
    {
        Play(_defaultFrames, _frameRate, _loop, onComplete: onComplete);
    }

    /// <summary>
    /// 播放指定序列帧。
    /// </summary>
    public void Play(Sprite[] frames, float frameRate, bool loop, Action<int> onFrameChanged = null, Action onComplete = null)
    {
        Stop();

        if (_targetImage == null || frames == null || frames.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        _playRoutine = StartCoroutine(PlayRoutine(frames, frameRate, loop, onFrameChanged, onComplete));
    }

    /// <summary>
    /// 停止当前序列帧动画。
    /// </summary>
    public void Stop()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        _isPlaying = false;

        if (_hideImageWhenStopped)
        {
            SetImageActive(false);
        }
    }

    /// <summary>
    /// 逐帧写入 Sprite，支持循环和单次播放。
    /// </summary>
    private IEnumerator PlayRoutine(Sprite[] frames, float frameRate, bool loop, Action<int> onFrameChanged, Action onComplete)
    {
        _isPlaying = true;
        SetImageActive(true);

        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                ApplyFrame(frames[i]);
                onFrameChanged?.Invoke(i);
                yield return WaitFrame(GetFrameDuration(frameRate));
            }
        } while (loop);

        _isPlaying = false;
        _playRoutine = null;
        onComplete?.Invoke();
    }

    /// <summary>
    /// 应用当前帧 Sprite 到目标 Image。
    /// </summary>
    private void ApplyFrame(Sprite frame)
    {
        _targetImage.sprite = frame;

        if (_setNativeSizeEachFrame)
        {
            _targetImage.SetNativeSize();
        }
    }

    /// <summary>
    /// 获取单帧等待时间，防止帧率配置为 0。
    /// </summary>
    private float GetFrameDuration(float frameRate)
    {
        return 1f / Mathf.Max(1f, frameRate);
    }

    /// <summary>
    /// 根据配置返回受 Time.timeScale 影响或不受影响的等待对象。
    /// </summary>
    private object WaitFrame(float frameDuration)
    {
        if (_useRealtime)
        {
            return new WaitForSecondsRealtime(frameDuration);
        }

        return new WaitForSeconds(frameDuration);
    }

    /// <summary>
    /// 显示或隐藏目标 Image。
    /// </summary>
    private void SetImageActive(bool isActive)
    {
        if (_targetImage != null)
        {
            _targetImage.gameObject.SetActive(isActive);
        }
    }
}
}
