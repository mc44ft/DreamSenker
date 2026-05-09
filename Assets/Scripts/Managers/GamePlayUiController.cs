using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSenker.UI.Panels;
using DreamSenker.UI.Panels.Inventory;

namespace DreamSenker.Managers
{
public class GamePlayUiController : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<EmptyEventArgs>(E_EventType.InputUI_SettingsPanel, OnSettingsPanel);
        EventCenter.Instance.AddEventListener<EmptyEventArgs>(E_EventType.InputUI_PackagePanel, OnPackagePanel);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<EmptyEventArgs>(E_EventType.InputUI_SettingsPanel, OnSettingsPanel);
        EventCenter.Instance.RemoveEventListener<EmptyEventArgs>(E_EventType.InputUI_PackagePanel, OnPackagePanel);
    }

    private void OnSettingsPanel(object eventSender, EmptyEventArgs args)
    {
        if (UIManager.Instance.CheckPanelIsShowing<SettingsPanel>())
        {
            //如果开启中 就关闭
            UIManager.Instance.HidePanel<SettingsPanel>();
        }
        else
        {
            //如果关闭中 就开启
            UIManager.Instance.ShowPanel<SettingsPanel>(E_UILayer.Middle);
        }
    }

    private void OnPackagePanel(object eventSender, EmptyEventArgs args)
    {
        Debug.Log("背包操作");
        if (UIManager.Instance.CheckPanelIsShowing<PackagePanel>())
        {
            //如果开启中 就关闭
            UIManager.Instance.HidePanel<PackagePanel>();
        }
        else
        {
            //如果关闭中 就开启
            UIManager.Instance.ShowPanel<PackagePanel>(E_UILayer.Middle);
        }
    }
}
}
