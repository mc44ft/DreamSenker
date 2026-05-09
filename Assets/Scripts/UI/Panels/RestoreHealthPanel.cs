using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DreamSenker.Managers;
using DreamSenker.Shared;

namespace DreamSenker.UI.Panels
{
public class RestoreHealthPanel : PanelBase_Mini
{
    [SerializeField] private Button _appectButton;
    [SerializeField] private Button _rejectButton;

    private void Start()
    {
        _appectButton.onClick.AddListener(OnAppect);
        _rejectButton.onClick.AddListener(OnReject);
    }

    
    private void OnAppect()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        //触发回血加成
        EventCenter.Instance.EventTrigger(E_EventType.Game_BonusEffect, this, new GameBonusEffectEventArgs(EBonusEffectType.RestoreHealth, 50));
        //通知Dialogue
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs(0));
        //关闭面板
        UIManager.Instance.HidePanel<RestoreHealthPanel>();
    }
    private void OnReject()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        //通知外部
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs(1));

        //关闭面板
        UIManager.Instance.HidePanel<RestoreHealthPanel>();
    }

    public override void OnShowFadePreComplete()
    {
        if(_appectButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(_appectButton.gameObject);
        }
    }
    public override void OnHideFadedComplete()
    {
        
    }

    
}
}
