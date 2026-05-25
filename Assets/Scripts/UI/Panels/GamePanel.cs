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

    //这里选择在Awake和OnDestroy中进行事件监听的订阅 是因为GamePanel需要在失活状态下也能响应面板更新
    //比如血量更新、TopBar更新
    private void Awake()
    {
        RemoveListener();
        AddListener();
    }

    private void OnDestroy()
    {
        RemoveListener();
    }

    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        
    }

    private void AddListener()
    {
        EventCenter.Instance.AddEventListener<PlayerHealthUpdateEventArgs>(EEventType.Player_HealthUpdate, OnPlayerGetHit);
        EventCenter.Instance.AddEventListener<StringEventArgs>(EEventType.Quest_TrackChanged, OnTrackChanged);
    }
    
    private void RemoveListener()
    {
        EventCenter.Instance.RemoveEventListener<PlayerHealthUpdateEventArgs>(EEventType.Player_HealthUpdate, OnPlayerGetHit);
        EventCenter.Instance.RemoveEventListener<StringEventArgs>(EEventType.Quest_TrackChanged, OnTrackChanged);
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
