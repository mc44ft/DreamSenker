using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Managers;
using DreamSeeker.QuestSystem;
using DreamSeeker.Shared;

namespace DreamSeeker.UI
{
public class GamePanel : PanelBase_Mini
{
    [SerializeField] private Image _hpImage;
    [SerializeField] private GameObject _topBar;
    [SerializeField] private TextMeshProUGUI _topBarText;
    

    public override void OnHideFadedComplete()
    {
        RemoveListener();
    }

    public override void OnShowFadePreComplete()
    {
        RemoveListener();
        AddListener();
    }

    private void AddListener()
    {
        EventCenter.Instance.AddEventListener<PlayerHealthUpdateEventArgs>(E_EventType.Player_HealthUpdate, OnPlayerGetHit);
        EventCenter.Instance.AddEventListener<StringEventArgs>(E_EventType.Quest_TrackChanged, OnTrackChanged);
    }
    
    private void RemoveListener()
    {
        EventCenter.Instance.RemoveEventListener<PlayerHealthUpdateEventArgs>(E_EventType.Player_HealthUpdate, OnPlayerGetHit);
        EventCenter.Instance.RemoveEventListener<StringEventArgs>(E_EventType.Quest_TrackChanged, OnTrackChanged);
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
    private void OnTrackChanged(object eventSender, StringEventArgs args)
    {
        if (QuestManager.Instance.TryGetQuestDefinition(args.Value, out var questDefinition))
        {
            SetTopBarActive(true, questDefinition.TopBarDescription);
        }
        
    }
    
}
}
