using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Managers;

namespace DreamSenker.UI.Panels
{
public class SettingsPanel : PanelBase_Mini
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _configPanelButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _closeButton;

    
    
    private void Start()
    {
        _resumeButton.onClick.AddListener(OnResume);
        _configPanelButton.onClick.AddListener(OnConfigPanel);
        _quitButton.onClick.AddListener(OnQuit);
        _closeButton.onClick.AddListener(OnClose);
    }
    private void OnDestroy()
    {
        _resumeButton.onClick.RemoveListener(OnResume);
        _configPanelButton.onClick.RemoveListener(OnConfigPanel);
        _quitButton.onClick.RemoveListener(OnQuit);
        _closeButton.onClick.RemoveListener(OnClose);
    }
    private void OnResume()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<SettingsPanel>();
    }
    private void OnConfigPanel()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.ShowPanel<ConfigPanel>(E_UILayer.Top);
    }
    private void OnQuit()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        Application.Quit();
    }
    private void OnClose()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<SettingsPanel>();
    }
    public override void OnHideFadedComplete()
    {
        //启用玩家输入
        InputManager.Instance.SetPlayerInputAction(true);
        //恢复游戏
        Time.timeScale = 1f;
    }

    public override void OnShowFadePreComplete()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiShowPanelClip);

        //禁用玩家输入
        InputManager.Instance.SetPlayerInputAction(false);
        //暂停游戏
        Time.timeScale = 0f;
    }
}
}
