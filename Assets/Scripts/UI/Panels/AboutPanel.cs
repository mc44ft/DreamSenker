using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Managers;

namespace DreamSeeker.UI.Panels
{
public class AboutPanel : PanelBase_Mini
{
    [SerializeField] private Button _closeButton;
    private void OnClose()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<AboutPanel>();
    }
    public override void OnHideFadedComplete()
    {
        _closeButton.onClick.RemoveListener(OnClose);
    }

    public override void OnShowFadePreComplete()
    {
        _closeButton.onClick.RemoveListener(OnClose);
        _closeButton.onClick.AddListener(OnClose);
        AudioManager.Instance.PlaySound(GameResources.Instance.UiShowPanelClip);
    }
}
}
