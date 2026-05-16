using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI.Panels.MainMenu
{
public class UIIconAlphaFlicker : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Graphic _targetGraphic;//需要闪烁的 UI 图形，留空时自动取当前物体上的 Graphic

    [Header("Mode")]
    [SerializeField] private EUIIconFlickerMode _flickerMode = EUIIconFlickerMode.Normal;//当前闪烁模式

    [Header("Mode Settings")]
    [SerializeField] private SolidColorSettings _solidColorSettings = new SolidColorSettings();//不闪烁模式颜色参数
    [SerializeField] private NormalFlickerSettings _normalSettings = new NormalFlickerSettings();//普通白色明暗闪烁参数
    [SerializeField] private PoliceLightFlickerSettings _policeLightSettings = new PoliceLightFlickerSettings();//红蓝警灯闪烁参数
    [SerializeField] private NightClubFlickerSettings _nightClubSettings = new NightClubFlickerSettings();//夜店彩色流转闪烁参数

    [Header("Playback")]
    [SerializeField] private bool _useRealtime = true;//是否忽略 Time.timeScale
    [SerializeField] private bool _playOnEnable = true;//启用组件时是否自动开始闪烁
    [SerializeField] private bool _resetAlphaOnDisable = true;//禁用组件时是否恢复亮态

    private Sequence _flickerSequence;//当前闪烁序列
    private Color _defaultColor;//组件启动时记录的原始颜色
    private bool _hasDefaultColor;//是否已经缓存过原始颜色
    private bool _isPlaying;//当前是否由本组件控制显示效果

    /// <summary>
    /// 初始化目标 Graphic。
    /// </summary>
    private void Awake()
    {
        CacheTargetGraphic();
        CacheDefaultColor();
    }

    /// <summary>
    /// 组件启用时按配置启动闪烁。
    /// </summary>
    private void OnEnable()
    {
        CacheTargetGraphic();

        if (_playOnEnable)
        {
            PlayFlicker();
        }
    }

    /// <summary>
    /// 组件禁用时停止闪烁并按配置恢复亮态。
    /// </summary>
    private void OnDisable()
    {
        StopFlicker(_resetAlphaOnDisable);
    }

    /// <summary>
    /// Inspector 修改数值时限制参数范围，PlayMode 下立即刷新当前闪烁效果。
    /// </summary>
    private void OnValidate()
    {
        EnsureSettings();
        _solidColorSettings.Validate();
        _normalSettings.Validate();
        _policeLightSettings.Validate();
        _nightClubSettings.Validate();

        if (Application.isPlaying && isActiveAndEnabled && (_isPlaying || _playOnEnable))
        {
            PlayFlicker();
        }
    }

    /// <summary>
    /// 开始循环播放当前模式的 Icon 闪烁。
    /// </summary>
    public void PlayFlicker()
    {
        CacheTargetGraphic();
        CacheDefaultColor();
        EnsureSettings();
        StopFlicker(false);

        if (_targetGraphic == null)
        {
            return;
        }

        _isPlaying = true;

        if (_flickerMode == EUIIconFlickerMode.SolidColor)
        {
            SetColor(_solidColorSettings.Color);
            return;
        }

        _flickerSequence = DOTween.Sequence();
        _flickerSequence.SetUpdate(_useRealtime);

        BuildCurrentModeSequence(_flickerSequence);

        _flickerSequence.SetLoops(-1, LoopType.Restart);
    }

    /// <summary>
    /// 停止 Icon 闪烁。
    /// </summary>
    public void StopFlicker(bool resetAlpha = true)
    {
        if (_flickerSequence != null)
        {
            _flickerSequence.Kill();
            _flickerSequence = null;
        }

        _isPlaying = false;

        if (resetAlpha && _hasDefaultColor)
        {
            SetColor(_defaultColor);
        }
    }

    /// <summary>
    /// 设置当前闪烁模式并立即重播。
    /// </summary>
    public void SetMode(EUIIconFlickerMode flickerMode)
    {
        _flickerMode = flickerMode;

        if (isActiveAndEnabled)
        {
            PlayFlicker();
        }
    }

    /// <summary>
    /// 缓存当前物体上的 UI Graphic。
    /// </summary>
    private void CacheTargetGraphic()
    {
        if (_targetGraphic == null)
        {
            _targetGraphic = GetComponent<Graphic>();
        }
    }

    /// <summary>
    /// 缓存 Icon 原始颜色，用于停止时恢复。
    /// </summary>
    private void CacheDefaultColor()
    {
        if (_targetGraphic != null)
        {
            _defaultColor = _targetGraphic.color;
            _hasDefaultColor = true;
        }
    }

    /// <summary>
    /// 补齐模式配置，兼容脚本升级后的旧序列化数据。
    /// </summary>
    private void EnsureSettings()
    {
        if (_solidColorSettings == null)
        {
            _solidColorSettings = new SolidColorSettings();
        }

        if (_normalSettings == null)
        {
            _normalSettings = new NormalFlickerSettings();
        }

        if (_policeLightSettings == null)
        {
            _policeLightSettings = new PoliceLightFlickerSettings();
        }

        if (_nightClubSettings == null)
        {
            _nightClubSettings = new NightClubFlickerSettings();
        }
    }

    /// <summary>
    /// 按当前模式构建闪烁序列。
    /// </summary>
    private void BuildCurrentModeSequence(Sequence sequence)
    {
        switch (_flickerMode)
        {
            case EUIIconFlickerMode.SolidColor:
                SetColor(_solidColorSettings.Color);
                break;
            case EUIIconFlickerMode.PoliceLight:
                BuildPoliceLightSequence(sequence);
                break;
            case EUIIconFlickerMode.NightClub:
                BuildNightClubSequence(sequence);
                break;
            default:
                BuildNormalSequence(sequence);
                break;
        }
    }

    /// <summary>
    /// 构建普通白色明暗闪烁序列。
    /// </summary>
    private void BuildNormalSequence(Sequence sequence)
    {
        SetColor(WithAlpha(_normalSettings.BrightColor, _normalSettings.BrightAlpha));
        sequence.Append(TweenColor(WithAlpha(_normalSettings.BrightColor, _normalSettings.DarkAlpha), _normalSettings.FadeOutDuration, _normalSettings.Ease));
        sequence.AppendInterval(_normalSettings.DarkHoldDuration);
        sequence.Append(TweenColor(WithAlpha(_normalSettings.BrightColor, _normalSettings.BrightAlpha), _normalSettings.FadeInDuration, _normalSettings.Ease));
        sequence.AppendInterval(_normalSettings.BrightHoldDuration);
    }

    /// <summary>
    /// 构建红蓝警灯闪烁序列。
    /// </summary>
    private void BuildPoliceLightSequence(Sequence sequence)
    {
        SetColor(WithAlpha(_policeLightSettings.RedColor, _policeLightSettings.BrightAlpha));

        if (_policeLightSettings.FlashCountPerColor <= 0)
        {
            AppendPoliceLightSolidColor(sequence, _policeLightSettings.RedColor);
            sequence.Append(TweenColor(WithAlpha(_policeLightSettings.BlueColor, _policeLightSettings.BrightAlpha), _policeLightSettings.SwitchDuration, _policeLightSettings.Ease));
            AppendPoliceLightSolidColor(sequence, _policeLightSettings.BlueColor);
            sequence.Append(TweenColor(WithAlpha(_policeLightSettings.RedColor, _policeLightSettings.BrightAlpha), _policeLightSettings.SwitchDuration, _policeLightSettings.Ease));
            return;
        }

        for (int i = 0; i < _policeLightSettings.FlashCountPerColor; i++)
        {
            AppendPoliceLightFlash(sequence, _policeLightSettings.RedColor);
        }

        sequence.Append(TweenColor(WithAlpha(_policeLightSettings.BlueColor, _policeLightSettings.BrightAlpha), _policeLightSettings.SwitchDuration, _policeLightSettings.Ease));

        for (int i = 0; i < _policeLightSettings.FlashCountPerColor; i++)
        {
            AppendPoliceLightFlash(sequence, _policeLightSettings.BlueColor);
        }

        sequence.Append(TweenColor(WithAlpha(_policeLightSettings.RedColor, _policeLightSettings.BrightAlpha), _policeLightSettings.SwitchDuration, _policeLightSettings.Ease));
    }

    /// <summary>
    /// 追加一次无爆闪的警灯颜色保持。
    /// </summary>
    private void AppendPoliceLightSolidColor(Sequence sequence, Color color)
    {
        sequence.AppendCallback(() => SetColor(WithAlpha(color, _policeLightSettings.BrightAlpha)));
        sequence.AppendInterval(_policeLightSettings.FlashHoldDuration);
    }

    /// <summary>
    /// 追加一次警灯爆闪。
    /// </summary>
    private void AppendPoliceLightFlash(Sequence sequence, Color color)
    {
        sequence.Append(TweenColor(WithAlpha(color, _policeLightSettings.DarkAlpha), _policeLightSettings.FadeDuration, _policeLightSettings.Ease));
        sequence.Append(TweenColor(WithAlpha(color, _policeLightSettings.BrightAlpha), _policeLightSettings.FadeDuration, _policeLightSettings.Ease));
        sequence.AppendInterval(_policeLightSettings.FlashHoldDuration);
    }

    /// <summary>
    /// 构建夜店彩色流转闪烁序列。
    /// </summary>
    private void BuildNightClubSequence(Sequence sequence)
    {
        Color[] colors = _nightClubSettings.Colors;
        if (colors == null || colors.Length == 0)
        {
            colors = NightClubFlickerSettings.DefaultColors;
        }

        SetColor(WithAlpha(colors[0], _nightClubSettings.BrightAlpha));

        for (int i = 0; i < colors.Length; i++)
        {
            Color brightColor = WithAlpha(colors[i], _nightClubSettings.BrightAlpha);
            Color darkColor = WithAlpha(colors[i], _nightClubSettings.DarkAlpha);

            sequence.Append(TweenColor(brightColor, _nightClubSettings.ColorFadeDuration, _nightClubSettings.Ease));
            sequence.Append(TweenColor(darkColor, _nightClubSettings.AlphaFadeDuration, _nightClubSettings.Ease));
            sequence.Append(TweenColor(brightColor, _nightClubSettings.AlphaFadeDuration, _nightClubSettings.Ease));
            sequence.AppendInterval(_nightClubSettings.ColorHoldDuration);
        }
    }

    /// <summary>
    /// 创建颜色过渡 Tween。
    /// </summary>
    private Tween TweenColor(Color targetColor, float duration, Ease ease)
    {
        return DOTween.To(GetColor, SetColor, targetColor, duration).SetEase(ease);
    }

    /// <summary>
    /// 获取当前 Graphic 颜色。
    /// </summary>
    private Color GetColor()
    {
        if (_targetGraphic == null)
        {
            return Color.white;
        }

        return _targetGraphic.color;
    }

    /// <summary>
    /// 设置当前 Graphic 颜色。
    /// </summary>
    private void SetColor(Color color)
    {
        if (_targetGraphic == null)
        {
            return;
        }

        color.a = Mathf.Clamp01(color.a);
        _targetGraphic.color = color;
    }

    /// <summary>
    /// 返回带指定透明度的颜色。
    /// </summary>
    private Color WithAlpha(Color color, float alpha)
    {
        color.a = Mathf.Clamp01(alpha);
        return color;
    }
}

public enum EUIIconFlickerMode
{
    Normal,//默认白色明暗闪烁
    PoliceLight,//红蓝警灯闪烁
    NightClub,//彩色流转闪烁
    SolidColor,//不闪烁，仅显示指定颜色
}

[Serializable]
public class SolidColorSettings
{
    [SerializeField] private Color _color = Color.white;//不闪烁模式显示颜色

    public Color Color => _color;

    /// <summary>
    /// 限制不闪烁模式颜色透明度范围。
    /// </summary>
    public void Validate()
    {
        _color.a = Mathf.Clamp01(_color.a);
    }
}

[Serializable]
public class NormalFlickerSettings
{
    [SerializeField] private Color _brightColor = Color.white;//普通模式使用的主颜色
    [SerializeField, Range(0f, 1f)] private float _darkAlpha = 0.35f;//变暗时透明度
    [SerializeField, Range(0f, 1f)] private float _brightAlpha = 1f;//变亮时透明度
    [SerializeField] private float _fadeOutDuration = 0.08f;//从亮到暗的时间
    [SerializeField] private float _fadeInDuration = 0.12f;//从暗到亮的时间
    [SerializeField] private float _darkHoldDuration = 0.04f;//保持暗态的时间
    [SerializeField] private float _brightHoldDuration = 0.45f;//保持亮态的时间
    [SerializeField] private Ease _ease = Ease.InOutSine;//普通模式透明度缓动

    public Color BrightColor => _brightColor;
    public float DarkAlpha => _darkAlpha;
    public float BrightAlpha => _brightAlpha;
    public float FadeOutDuration => _fadeOutDuration;
    public float FadeInDuration => _fadeInDuration;
    public float DarkHoldDuration => _darkHoldDuration;
    public float BrightHoldDuration => _brightHoldDuration;
    public Ease Ease => _ease;

    /// <summary>
    /// 限制普通模式参数范围。
    /// </summary>
    public void Validate()
    {
        _darkAlpha = Mathf.Clamp01(_darkAlpha);
        _brightAlpha = Mathf.Clamp01(_brightAlpha);
        _fadeOutDuration = Mathf.Max(0.01f, _fadeOutDuration);
        _fadeInDuration = Mathf.Max(0.01f, _fadeInDuration);
        _darkHoldDuration = Mathf.Max(0f, _darkHoldDuration);
        _brightHoldDuration = Mathf.Max(0f, _brightHoldDuration);
    }
}

[Serializable]
public class PoliceLightFlickerSettings
{
    [SerializeField] private Color _redColor = Color.red;//警灯模式红色
    [SerializeField] private Color _blueColor = Color.blue;//警灯模式蓝色
    [SerializeField, Range(0f, 1f)] private float _darkAlpha = 0.12f;//警灯变暗时透明度
    [SerializeField, Range(0f, 1f)] private float _brightAlpha = 1f;//警灯变亮时透明度
    [SerializeField] private float _fadeDuration = 0.045f;//单次闪烁明暗过渡时间
    [SerializeField] private float _switchDuration = 0.04f;//红蓝切换时间
    [SerializeField] private float _flashHoldDuration = 0.03f;//每次爆闪后的亮态保持时间
    [SerializeField] private int _flashCountPerColor = 2;//每个颜色连续爆闪次数，0 表示只做红蓝过渡不爆闪
    [SerializeField] private Ease _ease = Ease.OutQuad;//警灯模式缓动

    public Color RedColor => _redColor;
    public Color BlueColor => _blueColor;
    public float DarkAlpha => _darkAlpha;
    public float BrightAlpha => _brightAlpha;
    public float FadeDuration => _fadeDuration;
    public float SwitchDuration => _switchDuration;
    public float FlashHoldDuration => _flashHoldDuration;
    public int FlashCountPerColor => _flashCountPerColor;
    public Ease Ease => _ease;

    /// <summary>
    /// 限制警灯模式参数范围。
    /// </summary>
    public void Validate()
    {
        _darkAlpha = Mathf.Clamp01(_darkAlpha);
        _brightAlpha = Mathf.Clamp01(_brightAlpha);
        _fadeDuration = Mathf.Max(0.01f, _fadeDuration);
        _switchDuration = Mathf.Max(0.01f, _switchDuration);
        _flashHoldDuration = Mathf.Max(0f, _flashHoldDuration);
        _flashCountPerColor = Mathf.Max(0, _flashCountPerColor);
    }
}

[Serializable]
public class NightClubFlickerSettings
{
    public static readonly Color[] DefaultColors =
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.blue,
        Color.magenta,
    };//夜店模式默认彩色循环

    [SerializeField] private Color[] _colors =
    {
        Color.red,
        Color.yellow,
        Color.green,
        Color.cyan,
        Color.blue,
        Color.magenta,
    };//夜店模式流转颜色列表
    [SerializeField, Range(0f, 1f)] private float _darkAlpha = 0.35f;//夜店模式变暗时透明度
    [SerializeField, Range(0f, 1f)] private float _brightAlpha = 1f;//夜店模式变亮时透明度
    [SerializeField] private float _colorFadeDuration = 0.08f;//切到下一个颜色的时间
    [SerializeField] private float _alphaFadeDuration = 0.05f;//单个颜色明暗闪烁时间
    [SerializeField] private float _colorHoldDuration = 0.03f;//单个颜色亮态保持时间
    [SerializeField] private Ease _ease = Ease.Linear;//夜店模式缓动

    public Color[] Colors => _colors;
    public float DarkAlpha => _darkAlpha;
    public float BrightAlpha => _brightAlpha;
    public float ColorFadeDuration => _colorFadeDuration;
    public float AlphaFadeDuration => _alphaFadeDuration;
    public float ColorHoldDuration => _colorHoldDuration;
    public Ease Ease => _ease;

    /// <summary>
    /// 限制夜店模式参数范围。
    /// </summary>
    public void Validate()
    {
        _darkAlpha = Mathf.Clamp01(_darkAlpha);
        _brightAlpha = Mathf.Clamp01(_brightAlpha);
        _colorFadeDuration = Mathf.Max(0.01f, _colorFadeDuration);
        _alphaFadeDuration = Mathf.Max(0.01f, _alphaFadeDuration);
        _colorHoldDuration = Mathf.Max(0f, _colorHoldDuration);
    }
}
}
