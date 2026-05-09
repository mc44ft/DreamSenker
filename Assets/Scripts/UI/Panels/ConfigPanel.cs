using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Managers;

namespace DreamSenker.UI.Panels
{
public class ConfigPanel : PanelBase_Mini
{
    [SerializeField] private Button _closeButton;

    private void Start()
    {
        _closeButton.onClick.AddListener(OnClose);
    }

    private void OnClose()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<ConfigPanel>();
    }

    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiShowPanelClip);
    }
}
}
