using System;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

using DreamSeeker.Characters.Player;

namespace DreamSeeker.UI
{
public class BounsChoosePanel : PanelBase_Mini
{
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Button _sureButton;

    private Action<int> _onFinished;//面板关闭后返回给对话节点的结果回调

    /// <summary>
    /// 注入外部 UI 完成回调。
    /// </summary>
    public void Initialize(Action<int> onFinished)
    {
        _onFinished = onFinished;
    }

    private void SureButtonOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        Toggle activeToggle = _toggleGroup.GetFirstActiveToggle();
        if (activeToggle != null)
        {
            int index = activeToggle.transform.GetSiblingIndex();

            switch (index)
            {
                case 0:
                    // GameManager.Instance.Player.AddDebuff(true);
                    break;
                case 1:
                    GameManager.Instance.Player.DamageableHealth.Initialize((int)(GameManager.Instance.Player.DamageableHealth.MaxHealthAmount * 1.2f), (int)(GameManager.Instance.Player.DamageableHealth.MaxHealthAmount * 1.2f));
                    break;
                case 2:
                    GameManager.Instance.Player.PlayerSaveData.AttackMultiply += 0.3f;
                    break;
                default:
                    break;
            }
        }
        ClosePanel(0);
    }

    /// <summary>
    /// 关闭面板，并在淡出完成后通知对话节点继续执行。
    /// </summary>
    private void ClosePanel(int resultIndex)
    {
        Action<int> finishedCallback = _onFinished;
        _onFinished = null;

        UIManager.Instance.HidePanel<BounsChoosePanel>(null, () =>
        {
            finishedCallback?.Invoke(resultIndex);
        });
    }
    public override void OnHideFadedComplete()
    {
        _sureButton.onClick.RemoveListener(SureButtonOnClick);
    }

    public override void OnShowFadePreComplete()
    {
        _sureButton.onClick.RemoveListener(SureButtonOnClick);
        _sureButton.onClick.AddListener(SureButtonOnClick);
    }

    
}
}
