using System;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Data;
using DreamSeeker.Managers;
using UnityEngine.Serialization;

namespace DreamSeeker.UI
{
public class BeginPanel : PanelBase_Mini
{

    [FormerlySerializedAs("_playerButton")] [SerializeField] private Button _playButton;
    [SerializeField] private Button _configButton;
    [SerializeField] private Button _aboutButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _deleteDataButton;
    public override void OnShowFadePreComplete()
    {
        RemoveButtonListeners();
        AddButtonListeners();
    }

    public override void OnHideFadedComplete()
    {
        RemoveButtonListeners();
    }

    /// <summary>
    /// 绑定开始菜单按钮监听。
    /// </summary>
    private void AddButtonListeners()
    {
        _playButton.onClick.AddListener(OnPlay);
        _configButton.onClick.AddListener(OnConfig);
        _aboutButton.onClick.AddListener(OnAbout);
        _quitButton.onClick.AddListener(OnQuit);
        _deleteDataButton.onClick.AddListener(OnDeleteData);
    }

    /// <summary>
    /// 移除开始菜单按钮监听。
    /// </summary>
    private void RemoveButtonListeners()
    {
        _playButton.onClick.RemoveListener(OnPlay);
        _configButton.onClick.RemoveListener(OnConfig);
        _aboutButton.onClick.RemoveListener(OnAbout);
        _quitButton.onClick.RemoveListener(OnQuit);
        _deleteDataButton.onClick.RemoveListener(OnDeleteData);
    }
    private void OnPlay()
    {
        SceneTransition.Instance.ResetLoading(Resources.Load<Sprite>("LoadingCampMap"), 1);

        GameManager.Instance.LoadGame();
        

        UIManager.Instance.HidePanel<BeginPanel>();
    }
    private void OnConfig()
    {
        UIManager.Instance.ShowPanel<ConfigPanel>(E_UILayer.Middle);
    }
    private void OnAbout()
    {
        UIManager.Instance.ShowPanel<AboutPanel>(E_UILayer.Middle);
    }
    private void OnQuit()
    {
        Application.Quit();
    }
    private void OnDeleteData()
    {
        RunningDataManager.Instance.DeleteData();
    }
}
}
