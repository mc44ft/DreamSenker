using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Managers;

namespace DreamSeeker.UI
{
public class SettingsPage : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _configPanelButton;
    [SerializeField] private Button _quitButton;
    
    /// <summary>
    /// 页面启用时绑定按钮监听。
    /// </summary>
    private void OnEnable()
    {
        AddButtonListeners();
    }

    /// <summary>
    /// 页面禁用时移除按钮监听。
    /// </summary>
    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    /// <summary>
    /// 绑定设置页按钮监听。
    /// </summary>
    private void AddButtonListeners()
    {
        RemoveButtonListeners();

        if (_resumeButton != null)
            _resumeButton.onClick.AddListener(OnResume);
        if (_configPanelButton != null)
            _configPanelButton.onClick.AddListener(OnConfigPanel);
        if (_quitButton != null)
            _quitButton.onClick.AddListener(OnQuit);
    }

    /// <summary>
    /// 移除设置页按钮监听。
    /// </summary>
    private void RemoveButtonListeners()
    {
        if (_resumeButton != null)
            _resumeButton.onClick.RemoveListener(OnResume);
        if (_configPanelButton != null)
            _configPanelButton.onClick.RemoveListener(OnConfigPanel);
        if (_quitButton != null)
            _quitButton.onClick.RemoveListener(OnQuit);
    }
    private void OnResume()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<MainMenuPanel>();
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
    
}
}
