using System;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Inventory;
using DreamSenker.Managers;
using DreamSenker.Shared;

namespace DreamSenker.UI.Panels
{
public class TaskPanel : PanelBase_Mini
{
    [Header("CONTAINER")]
    [SerializeField] private GameObject m_publishButtons;//发布任务面板的按钮集合
    [SerializeField] private GameObject m_deliverButtons;//提交任务面板的按钮集合

    [Header("ELEMENT")]
    [SerializeField] private Button m_acceptButton;
    [SerializeField] private Button m_rejectButton;
    [SerializeField] private Button m_deliverButton;

    public void Initialize(E_PanelMode mode)
    {
        m_publishButtons.SetActive(false);
        m_deliverButtons.SetActive(false);

        switch (mode)
        {
            case E_PanelMode.Publish:
                m_publishButtons.SetActive(true);
                m_acceptButton.onClick.AddListener(AcceptOnClick);
                m_rejectButton.onClick.AddListener(RejectOnClick);
                break;
            case E_PanelMode.Deliver:
                m_deliverButtons.SetActive(true);
                m_deliverButton.onClick.AddListener(DeliverOnClick);
                break;
            default:
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
