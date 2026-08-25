using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 作为所有UI面板的父类容器
/// 执行顺序（前四个同一帧先后执行）
/// 1.Awake
/// 2.OnShowPanelFadePre
/// 3.OnShowFadePreComplete
/// 4.Start
/// 5.OnShowPanelFadeed
/// </summary>
public interface IPanel
{
    

    //接口成员默认是public 和 abstract的
    /// <summary>
    /// 面板淡入动画结束前执行的逻辑 透明度为0
    /// 用于处理面板内部初始化逻辑
    /// 在两个回调之间执行
    /// </summary>
    void OnShowFadePreComplete();
    /// <summary>
    /// 面板淡出动画结束后的逻辑 透明度为0 
    /// 在HidePanel的回调之前执行
    /// 在两个回调之间执行
    /// </summary>
    void OnHideFadedComplete();
}
