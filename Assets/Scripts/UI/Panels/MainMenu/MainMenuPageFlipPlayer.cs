using System;
using UnityEngine;

namespace DreamSeeker.UI.Panels.MainMenu
{
public class MainMenuPageFlipPlayer : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private UIImageSpriteAnimator _spriteAnimator;//实际负责播放 UI Sprite 序列帧的通用播放器

    [Header("Frames")]
    [SerializeField] private Sprite[] _nextPageFrames;//下一页翻页序列帧
    [SerializeField] private Sprite[] _previousPageFrames;//上一页翻页序列帧

    [Header("Playback")]
    [SerializeField] private float _frameRate = 24f;//翻页动画播放帧率
    [SerializeField] private int _nextPageSwitchFrameIndex = -1;//下一页内容切换帧；-1 表示自动取中间帧
    [SerializeField] private int _previousPageSwitchFrameIndex = -1;//上一页内容切换帧；-1 表示自动取中间帧

    private bool _isPlaying;//当前是否正在播放翻页动画

    public bool IsPlaying => _isPlaying || (_spriteAnimator != null && _spriteAnimator.IsPlaying);

    /// <summary>
    /// 播放下一页翻页动画。
    /// </summary>
    public void PlayNext(Action onSwitchPage = null, Action onComplete = null)
    {
        Play(_nextPageFrames, _nextPageSwitchFrameIndex, onSwitchPage, onComplete);
    }

    /// <summary>
    /// 播放上一页翻页动画。
    /// </summary>
    public void PlayPrevious(Action onSwitchPage = null, Action onComplete = null)
    {
        Play(_previousPageFrames, _previousPageSwitchFrameIndex, onSwitchPage, onComplete);
    }

    /// <summary>
    /// 播放指定翻页序列帧，并在指定帧触发页面内容切换。
    /// </summary>
    public void Play(Sprite[] frames, int switchFrameIndex, Action onSwitchPage = null, Action onComplete = null)
    {
        Stop();

        if (_spriteAnimator == null || frames == null || frames.Length == 0)
        {
            onSwitchPage?.Invoke();
            onComplete?.Invoke();
            return;
        }

        bool hasSwitchedPage = false;
        int resolvedSwitchFrameIndex = ResolveSwitchFrameIndex(frames, switchFrameIndex);
        _isPlaying = true;
        
        _spriteAnimator.gameObject.SetActive(true);
        _spriteAnimator.Play(
            frames,
            _frameRate,
            loop: false,
            onFrameChanged: frameIndex =>
            {
                if (hasSwitchedPage || frameIndex < resolvedSwitchFrameIndex)
                {
                    return;
                }

                hasSwitchedPage = true;
                onSwitchPage?.Invoke();
            },
            onComplete: () =>
            {
                if (!hasSwitchedPage)
                {
                    onSwitchPage?.Invoke();
                }

                _isPlaying = false;
                onComplete?.Invoke();
                _spriteAnimator.gameObject.SetActive(false);
            });
    }

    /// <summary>
    /// 停止当前翻页动画。
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _spriteAnimator?.Stop();
    }

    /// <summary>
    /// 组件禁用时停止翻页动画。
    /// </summary>
    private void OnDisable()
    {
        Stop();
    }

    /// <summary>
    /// 解析实际切换帧；未配置时取动画中间帧。
    /// </summary>
    private int ResolveSwitchFrameIndex(Sprite[] frames, int switchFrameIndex)
    {
        if (switchFrameIndex < 0)
        {
            return frames.Length / 2;
        }

        return Mathf.Clamp(switchFrameIndex, 0, frames.Length - 1);
    }
}
}
