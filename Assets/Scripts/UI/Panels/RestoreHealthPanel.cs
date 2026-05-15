using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.UI.Panels
{
public class RestoreHealthPanel : PanelBase_Mini
{
    [SerializeField] private Button _appectButton;
    [SerializeField] private Button _rejectButton;

    private Action<int> _onFinished;//面板关闭后返回给对话节点的结果回调

    /// <summary>
    /// 注入外部 UI 完成回调。
    /// </summary>
    public void Initialize(Action<int> onFinished)
    {
        _onFinished = onFinished;
    }

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
        ClosePanel(0);
    }
    private void OnReject()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        ClosePanel(1);
    }

    /// <summary>
    /// 关闭面板，并在淡出完成后通知对话节点继续执行。
    /// </summary>
    private void ClosePanel(int resultIndex)
    {
        Action<int> finishedCallback = _onFinished;
        _onFinished = null;

        UIManager.Instance.HidePanel<RestoreHealthPanel>(null, () =>
        {
            finishedCallback?.Invoke(resultIndex);
        });
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
