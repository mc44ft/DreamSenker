using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Managers;
using DreamSenker.Shared;

namespace DreamSenker.UI.Panels
{
public class GamePanel : PanelBase_Mini
{
    [SerializeField] private Image _hpImage;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private GameObject _topBar;
    [SerializeField] private TextMeshProUGUI _topBarText;

    private void Start()
    {
        _settingsButton.onClick.AddListener(OnSettings);
    }

    private void OnSettings()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.ShowPanel<SettingsPanel>(E_UILayer.Middle);
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<PlayerHealthUpdateEventArgs>(E_EventType.Player_HealthUpdate, OnPlayerGetHit);
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<PlayerHealthUpdateEventArgs>(E_EventType.Player_HealthUpdate, OnPlayerGetHit);
    }

    private void OnPlayerGetHit(object eventSender, PlayerHealthUpdateEventArgs args)
    {
        UpdateHpBar(args.MaxHealthAmount, args.CurrentHealthAmount);
    }
    private void UpdateHpBar(int MaxHealthAmount, int CurrentHealthAmount)
    {
        float percentAmount = (float)CurrentHealthAmount / MaxHealthAmount;
        _hpImage.fillAmount = percentAmount;
    }
    public void SetTopBarActive(bool active, string content = "")
    {
        _topBar.SetActive(active);
        if(active)
        {
            _topBarText.text = content;
        }
    }
    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        
    }
}
}
