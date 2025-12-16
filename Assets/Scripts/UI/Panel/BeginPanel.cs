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
        AudioManager.Instance.PlayMusic(GameResources.Instance.BeginPanelClip);

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
        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        SceneTransition.Instance.ResetLoading(Resources.Load<Sprite>("LoadingCampMap"), 1);


        bool hasSaveData = GameManager.Instance.LoadGame();

        //无存档数据才播放动画
        if(!hasSaveData)
        {
            //播放过场动画
            IntroController.Instance.PlayVideo(GameResources.Instance.BeginVideoClip, () =>
            {
                AudioManager.Instance.PlayMusic(GameResources.Instance.CommonMapClip);
            });
        }
        

        UIManager.Instance.HidePanel<BeginPanel>();
    }
    private void OnConfig()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.ShowPanel<ConfigPanel>(E_UILayer.Middle);
    }
    private void OnAbout()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.ShowPanel<AboutPanel>(E_UILayer.Middle);
    }
    private void OnQuit()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        Application.Quit();
    }
    private void OnDeleteData()
    {
        RunningDataManager.Instance.DeleteData();
    }
}
