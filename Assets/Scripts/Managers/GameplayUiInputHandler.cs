using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.UI;

namespace DreamSeeker.Managers
{
public class GameplayUiInputHandler : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<EmptyEventArgs>(EEventType.InputUI_MainMenuPanel, OnMainMenuPanel);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<EmptyEventArgs>(EEventType.InputUI_MainMenuPanel, OnMainMenuPanel);
    }
    

    private void OnMainMenuPanel(object eventSender, EmptyEventArgs args)
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
