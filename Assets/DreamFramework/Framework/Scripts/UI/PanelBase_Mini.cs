using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 简单的UI面板继承这个基类
/// 该面板预制体必须挂载CanvasGroup和Widget两个组件 用于控制淡入淡出
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(Widget))]
[DisallowMultipleComponent]
public abstract class PanelBase_Mini : MonoBehaviour, IPanel
{
    public abstract void OnShowFadePreComplete();
    public abstract void OnHideFadedComplete();
}
