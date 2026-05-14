using System;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Inventory;
using DreamSenker.Managers;
using DreamSenker.Shared;
using UnityEngine.Serialization;

namespace DreamSenker.UI.Panels
{
public class TaskPanel : PanelBase_Mini
{
    [Header("CONTAINER")]
    [SerializeField] private GameObject _publishButtons;//发布任务面板的按钮集合
    [SerializeField] private GameObject _deliverButtons;//提交任务面板的按钮集合
    
    [Header("ELEMENT")]
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _rejectButton;
    [SerializeField] private Button _deliverButton;

    public void Initialize(E_PanelMode mode)
    {
        _publishButtons.SetActive(false);
        _deliverButtons.SetActive(false);

        switch (mode)
        {
            case E_PanelMode.Publish:
                _publishButtons.SetActive(true);
                _acceptButton.onClick.RemoveListener(AcceptOnClick);
                _rejectButton.onClick.RemoveListener(RejectOnClick);
                _acceptButton.onClick.AddListener(AcceptOnClick);
                _rejectButton.onClick.AddListener(RejectOnClick);
                break;
            case E_PanelMode.Deliver:
                _deliverButtons.SetActive(true);
                _deliverButton.onClick.RemoveListener(DeliverOnClick);
                _deliverButton.onClick.AddListener(DeliverOnClick);
                break;
        }
    }
    private void AcceptOnClick()
    {
        //接受任务
        UIManager.Instance.GetPanel<GamePanel>((panel) =>
        {
            panel.SetTopBarActive(true, "寻找晨露梦核 位于树冠顶地图左侧");
        });

        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        //通知外部
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs(0));

        UIManager.Instance.HidePanel<TaskPanel>();
    }
    private void RejectOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        //通知外部
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs(1));

        UIManager.Instance.HidePanel<TaskPanel>();
    }
    private void DeliverOnClick()
    {
        //交付任务
        UIManager.Instance.GetPanel<GamePanel>((panel) =>
        {
            panel.SetTopBarActive(false);
        });

        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        //移除晨露梦核
        InventoryManager.Instance.RemoveItemFromPackage(EPackageItemType.Chen);
        //通知外部
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PanelFinished, this, new DialoguePanelFinishedEventArgs(0));

        UIManager.Instance.HidePanel<TaskPanel>();
    }
    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        
    }


    public enum E_PanelMode
    {
        /// <summary>
        /// 发布任务状态
        /// 接受/拒绝
        /// </summary>
        Publish,
        /// <summary>
        /// 交付任务状态
        /// 交付
        /// </summary>
        Deliver,
    }
}
}
