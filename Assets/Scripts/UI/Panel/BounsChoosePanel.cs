using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BounsChoosePanel : PanelBase_Mini
{
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Button _sureButton;

    private void Start()
    {
        _sureButton.onClick.AddListener(SureButtonOnClick);
    }
    private void OnDestroy()
    {
        _sureButton.onClick.RemoveListener(SureButtonOnClick);
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
        //触发外部面板操作完成事件
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs());

        UIManager.Instance.HidePanel<BounsChoosePanel>();
    }
    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        
    }

    
}
