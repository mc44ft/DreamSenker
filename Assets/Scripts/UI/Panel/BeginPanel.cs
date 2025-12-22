using System;
using UnityEngine;
using UnityEngine.UI;

public class BeginPanel : PanelBase_Mini
{

    [SerializeField] private Button _playerButton;
    [SerializeField] private Button _configButton;
    [SerializeField] private Button _aboutButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _deleteDataButton;
    public override void OnShowFadePreComplete()
    {
        _playerButton.onClick.AddListener(OnPlay);
        _configButton.onClick.AddListener(OnConfig);
        _aboutButton.onClick.AddListener(OnAbout);
        _quitButton.onClick.AddListener(OnQuit);
        _deleteDataButton.onClick.AddListener(OnDeleteData);
    }

    

    public override void OnHideFadedComplete()
    {
        _playerButton.onClick.RemoveListener(OnPlay);
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
