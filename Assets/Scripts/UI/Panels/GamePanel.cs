using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Characters.Player;
using DreamSeeker.Framework.Events;
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
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerGetHit);
        EventBus.Subscribe<QuestTrackChangedEvent>(OnTrackChanged);
    }
    
    private void RemoveListener()
    {
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerGetHit);
        EventBus.Unsubscribe<QuestTrackChangedEvent>(OnTrackChanged);
    }
    
    /// <summary>
    /// 根据玩家生命值事件刷新血条。
    /// </summary>
    private void OnPlayerGetHit(PlayerHealthChangedEvent eventData)
    {
        UpdateHpBar(eventData.MaxHealthAmount, eventData.CurrentHealthAmount);
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
    /// <summary>
    /// 根据追踪任务变化事件刷新顶部任务提示。
    /// </summary>
    private void OnTrackChanged(QuestTrackChangedEvent eventData)
    {
        if (QuestManager.Instance.TryGetQuestDefinition(eventData.QuestId, out var questDefinition))
        {
            SetTopBarActive(true, questDefinition.TopBarDescription);
        }
        
    }
    
}
}
