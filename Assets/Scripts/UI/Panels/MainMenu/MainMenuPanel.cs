using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using DreamSeeker.Characters.Player;
using DreamSeeker.Framework.Events;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.UI
{
public class MainMenuPanel : PanelBase_Mini
{
    [Header("Dock Toggles")]
    [SerializeField] private Toggle _characterToggle;//人物页 Dock Toggle
    [SerializeField] private Toggle _packageToggle;//背包页 Dock Toggle
    [SerializeField] private Toggle _questToggle;//任务页 Dock Toggle
    [SerializeField] private Toggle _settingsToggle;//设置页 Dock Toggle
    [SerializeField] private Toggle _saveToggle;//保存页 Dock Toggle
    [SerializeField] private Toggle _iconFlickerSwitchToggle;//控制 Icon 闪烁目标显隐的 Toggle
    [Header("Buttons")]
    [SerializeField] private Button _closeButton;//关闭总菜单按钮

    [Header("Effects")]
    [SerializeField] private UIIconAlphaFlicker _lightAlphaFlicker;//由 Toggle 控制显隐的 Icon 闪烁组件

    
    
    [Header("Pages")]
    [SerializeField] private GameObject _characterPage;//人物页根物体
    [SerializeField] private GameObject _packagePage;//背包页根物体
    [SerializeField] private GameObject _questPage;//任务页根物体
    [SerializeField] private GameObject _settingsPage;//设置页根物体
    [SerializeField] private GameObject _savePage;//保存页根物体

    [Header("Page Flip")]
    [SerializeField] private MainMenuPageFlipPlayer _pageFlipPlayer;//切页时播放的翻页动画
    [SerializeField] private EMainMenuPage _defaultPage = EMainMenuPage.Character;//打开面板时默认显示的页面

    public PackagePage PackagePage => _packagePageComponent;
    public SettingsPage SettingsPage => _settingsPageComponent;

    private PackagePage _packagePageComponent;
    private SettingsPage _settingsPageComponent;
    
    private EMainMenuPage _currentPage;//当前显示的页面
    private bool _isInitialized;//是否完成首次页面初始化

#region Unity Lifecycle

    private void Awake()
    {
        _packagePageComponent = _packagePage != null ? _packagePage.GetComponent<PackagePage>() : null;
        _settingsPageComponent = _settingsPage != null ? _settingsPage.GetComponent<SettingsPage>() : null;
    }

#endregion

#region Panel Lifecycle

    public override void OnShowFadePreComplete()
    {
        AddUIListeners();

        if (!_isInitialized)
        {
            ShowPageImmediately(_defaultPage);
        }

        SyncIconFlickerSwitchToggle();

        AudioManager.Instance.PlaySound(GameResources.Instance.UiShowPanelClip);

        //禁用玩家输入
        InputManager.Instance.SetPlayerInputAction(false);
        //暂停游戏
        Time.timeScale = 0f;
    }

    public override void OnHideFadedComplete()
    {
        RemoveUIListeners();

        //启用玩家输入
        InputManager.Instance.SetPlayerInputAction(true);
        //恢复游戏
        Time.timeScale = 1f;
    }

#endregion

#region UI Event Handlers

    /// <summary>
    /// 背包页 Toggle 选中时切换到背包页。
    /// </summary>
    private void OnBackpackToggleValueChanged(bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        SwitchPage(EMainMenuPage.package);
    }

    /// <summary>
    /// 任务页 Toggle 选中时切换到任务页。
    /// </summary>
    private void OnQuestToggleValueChanged(bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        SwitchPage(EMainMenuPage.Quest);
    }

    /// <summary>
    /// 人物页 Toggle 选中时切换到人物页。
    /// </summary>
    private void OnCharacterToggleValueChanged(bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        SwitchPage(EMainMenuPage.Character);
    }

    /// <summary>
    /// 设置页 Toggle 选中时切换到设置页。
    /// </summary>
    private void OnSettingsToggleValueChanged(bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        SwitchPage(EMainMenuPage.Settings);
    }

    /// <summary>
    /// 保存页 Toggle 选中时切换到保存页。
    /// </summary>
    private void OnSaveToggleValueChanged(bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        SwitchPage(EMainMenuPage.Save);
    }

    /// <summary>
    /// 关闭总菜单面板。
    /// </summary>
    private void OnCloseButtonClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
        UIManager.Instance.HidePanel<MainMenuPanel>();
    }

    /// <summary>
    /// 根据 Toggle 状态设置 Icon 闪烁组件的 TargetGraphic 显隐。
    /// </summary>
    private void OnIconFlickerSwitchToggleValueChanged(bool isOn)
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        if (_lightAlphaFlicker != null)
        {
            _lightAlphaFlicker.SetTargetGraphicVisible(isOn);
        }
    }

    /// <summary>
    /// 根据玩家血量比例切换灯光闪烁模式。
    /// </summary>
    private void OnPlayerHealthUpdate(PlayerHealthChangedEvent eventData)
    {
        SwitchLightMode((float)eventData.CurrentHealthAmount / eventData.MaxHealthAmount);
    }

    private void SwitchLightMode(float percentage)
    {
        if (percentage < 0.2f)
        {
            _lightAlphaFlicker.SetMode(EUIIconFlickerMode.PoliceLight);
        }
        else if (percentage >= 0.2f && percentage < 0.8f)
        {
            _lightAlphaFlicker.SetMode(EUIIconFlickerMode.Normal);
        }
        else
        {
            _lightAlphaFlicker.SetMode(EUIIconFlickerMode.SolidColor);
        }
    }

#endregion

#region Page Switching

    /// <summary>
    /// 按页面枚举切换子页面，必要时播放翻页动画。
    /// </summary>
    public void SwitchPage(EMainMenuPage targetPage)
    {
        if (!_isInitialized)
        {
            ShowPageImmediately(targetPage);
            return;
        }

        if (_currentPage == targetPage || (_pageFlipPlayer != null && _pageFlipPlayer.IsPlaying))
        {
            SyncDockToggles();
            return;
        }

        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
        PlayPageFlip(targetPage);
    }

    /// <summary>
    /// 不播放动画，直接显示指定页面。
    /// </summary>
    public void ShowPageImmediately(EMainMenuPage page)
    {
        SetPageActive(_packagePage, page == EMainMenuPage.package);
        SetPageActive(_questPage, page == EMainMenuPage.Quest);
        SetPageActive(_characterPage, page == EMainMenuPage.Character);
        SetPageActive(_settingsPage, page == EMainMenuPage.Settings);
        SetPageActive(_savePage, page == EMainMenuPage.Save);

        _currentPage = page;
        _isInitialized = true;
        SyncDockToggles();
    }

    /// <summary>
    /// 播放翻页动画，并在翻页中段切换页面内容。
    /// </summary>
    private void PlayPageFlip(EMainMenuPage targetPage)
    {
        if (_pageFlipPlayer == null)
        {
            ShowPageImmediately(targetPage);
            return;
        }

        if (IsForwardPage(targetPage))
        {
            _pageFlipPlayer.PlayNext(() => ShowPageImmediately(targetPage));
        }
        else
        {
            _pageFlipPlayer.PlayPrevious(() => ShowPageImmediately(targetPage));
        }
    }

    /// <summary>
    /// 判断目标页是否在当前页右侧，用于决定播放下一页还是上一页动画。
    /// </summary>
    private bool IsForwardPage(EMainMenuPage targetPage)
    {
        return GetPageIndex(targetPage) > GetPageIndex(_currentPage);
    }

    /// <summary>
    /// 获取页面顺序索引。
    /// </summary>
    private int GetPageIndex(EMainMenuPage page)
    {
        return page switch
        {
            EMainMenuPage.Character => 0,
            EMainMenuPage.package => 1,
            EMainMenuPage.Quest => 2,
            EMainMenuPage.Settings => 3,
            EMainMenuPage.Save => 4,
            _ => 0
        };
    }

    /// <summary>
    /// 设置页面根物体显隐。
    /// </summary>
    private void SetPageActive(GameObject pageRoot, bool isActive)
    {
        if (pageRoot != null)
        {
            pageRoot.SetActive(isActive);
        }
    }

#endregion

#region Toggle State Sync

    /// <summary>
    /// 同步 Dock Toggle 的选中状态。
    /// </summary>
    private void SyncDockToggles()
    {
        SetToggleIsOnWithoutNotify(_packageToggle, _currentPage == EMainMenuPage.package);
        SetToggleIsOnWithoutNotify(_questToggle, _currentPage == EMainMenuPage.Quest);
        SetToggleIsOnWithoutNotify(_characterToggle, _currentPage == EMainMenuPage.Character);
        SetToggleIsOnWithoutNotify(_settingsToggle, _currentPage == EMainMenuPage.Settings);
        SetToggleIsOnWithoutNotify(_saveToggle, _currentPage == EMainMenuPage.Save);
    }

    /// <summary>
    /// 同步 Icon 闪烁开关 Toggle 的选中状态。
    /// </summary>
    private void SyncIconFlickerSwitchToggle()
    {
        if (_lightAlphaFlicker != null)
        {
            SetToggleIsOnWithoutNotify(_iconFlickerSwitchToggle, _lightAlphaFlicker.IsTargetGraphicVisible);
        }
    }

    /// <summary>
    /// 安全设置 Toggle 状态，不触发回调。
    /// </summary>
    private void SetToggleIsOnWithoutNotify(Toggle toggle, bool isOn)
    {
        if (toggle != null)
        {
            toggle.SetIsOnWithoutNotify(isOn);
        }
    }

#endregion

#region Listener Binding

    /// <summary>
    /// 绑定 Toggle 和按钮监听。
    /// </summary>
    private void AddUIListeners()
    {
        RemoveUIListeners();

        AddToggleListener(_packageToggle, OnBackpackToggleValueChanged);
        AddToggleListener(_questToggle, OnQuestToggleValueChanged);
        AddToggleListener(_characterToggle, OnCharacterToggleValueChanged);
        AddToggleListener(_settingsToggle, OnSettingsToggleValueChanged);
        AddToggleListener(_saveToggle, OnSaveToggleValueChanged);
        AddToggleListener(_iconFlickerSwitchToggle, OnIconFlickerSwitchToggleValueChanged);
        AddButtonListener(_closeButton, OnCloseButtonClick);
        EventBus.Subscribe<PlayerHealthChangedEvent>(OnPlayerHealthUpdate);
    }

    /// <summary>
    /// 移除 Toggle 和按钮监听。
    /// </summary>
    private void RemoveUIListeners()
    {
        RemoveToggleListener(_packageToggle, OnBackpackToggleValueChanged);
        RemoveToggleListener(_questToggle, OnQuestToggleValueChanged);
        RemoveToggleListener(_characterToggle, OnCharacterToggleValueChanged);
        RemoveToggleListener(_settingsToggle, OnSettingsToggleValueChanged);
        RemoveToggleListener(_saveToggle, OnSaveToggleValueChanged);
        RemoveToggleListener(_iconFlickerSwitchToggle, OnIconFlickerSwitchToggleValueChanged);
        RemoveButtonListener(_closeButton, OnCloseButtonClick);
        EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnPlayerHealthUpdate);
    }

    /// <summary>
    /// 安全添加 Toggle 监听。
    /// </summary>
    private void AddToggleListener(Toggle toggle, UnityAction<bool> callback)
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(callback);
        }
    }

    /// <summary>
    /// 安全移除 Toggle 监听。
    /// </summary>
    private void RemoveToggleListener(Toggle toggle, UnityAction<bool> callback)
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(callback);
        }
    }

    /// <summary>
    /// 安全添加按钮监听。
    /// </summary>
    private void AddButtonListener(Button button, UnityAction callback)
    {
        if (button != null)
        {
            button.onClick.AddListener(callback);
        }
    }

    /// <summary>
    /// 安全移除按钮监听。
    /// </summary>
    private void RemoveButtonListener(Button button, UnityAction callback)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(callback);
        }
    }

#endregion

}

public enum EMainMenuPage
{
    Character,
    package,
    Quest,
    Settings,
    Save,
}
}
