using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.Framework.Events;
using DreamSeeker.UI;

namespace DreamSeeker.Managers
{
public class GameplayUiInputHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EventBus.Subscribe<MainMenuPanelRequestedEvent>(OnMainMenuPanel);
    }
    private void OnDisable()
    {
        EventBus.Unsubscribe<MainMenuPanelRequestedEvent>(OnMainMenuPanel);
    }
    

    /// <summary>
    /// 响应主菜单输入请求并切换面板显示状态。
    /// </summary>
    private void OnMainMenuPanel(MainMenuPanelRequestedEvent eventData)
    {
        Debug.Log("背包操作");
        if (UIManager.Instance.CheckPanelIsShowing<MainMenuPanel>())
        {
            //如果开启中 就关闭
            UIManager.Instance.HidePanel<MainMenuPanel>();
        }
        else
        {
            //如果关闭中 就开启
            UIManager.Instance.ShowPanel<MainMenuPanel>(E_UILayer.Middle);
        }
    }
}
}
