using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Managers;
using DreamSeeker.UI.Panels.MainMenu;

namespace DreamSeeker.UI.Panels
{
public class SettingsPage : MonoBehaviour
{
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _configPanelButton;
    [SerializeField] private Button _quitButton;
    
    private void Start()
    {
        _resumeButton.onClick.AddListener(OnResume);
        _configPanelButton.onClick.AddListener(OnConfigPanel);
        _quitButton.onClick.AddListener(OnQuit);
    }
    private void OnDestroy()
    {
        _resumeButton.onClick.RemoveListener(OnResume);
        _configPanelButton.onClick.RemoveListener(OnConfigPanel);
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
