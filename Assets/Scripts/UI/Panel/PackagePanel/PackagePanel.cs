using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackagePanel : PanelBase_Mini
{
    [SerializeField] private GameObject _itemGridPrefab;

    [SerializeField] private Transform _container;

    [Header("UI COMPONENT")]
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private TextMeshProUGUI _contentText;
    [SerializeField] private Button _sureButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;


    private List<PackagePanelItemIcon> _usedItemIconList = new List<PackagePanelItemIcon>();
    private PackagePanelItemIcon _currentSelectedItem;

    private void Start()
    {
        _sureButton.onClick.AddListener(SureButtonOnClick);
        _cancelButton.onClick.AddListener(CancelButtonOnClick);
        _closeButton.onClick.AddListener(CloseButtonOnClick);
    }
    private void OnDestroy()
    {
        _sureButton.onClick.RemoveListener(SureButtonOnClick);
        _cancelButton.onClick.RemoveListener(CancelButtonOnClick);
        _closeButton.onClick.RemoveListener(CloseButtonOnClick);
    }
    private void SureButtonOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        if(_currentSelectedItem != null)
        {
            //数据层移除
            //移除背包中该物品的数据
            InventoryManager.Instance.RemoveItemFromPackage(_currentSelectedItem.ID);
            //表现层移除
            _usedItemIconList.Remove( _currentSelectedItem );
            _currentSelectedItem.PushSelfToPool();

            //触发加成效果
            EventCenter.Instance.EventTrigger(
                E_EventType.Game_BonusEffect, this, 
                new GameBonusEffectEventArgs(_currentSelectedItem.BonusEffectType, _currentSelectedItem.BonusAmount));


            if(_usedItemIconList.Count == 0)
            {
                _contentText.text = "你的背包空空如也";
            }
        }
    }

    private void CancelButtonOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<PackagePanel>();
    }
    private void CloseButtonOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        UIManager.Instance.HidePanel<PackagePanel>();
    }
    private void SetDescriptionText(string content)
    {
        _contentText.text = content;
    }
    public override void OnHideFadedComplete()
    {
        Time.timeScale = 1f;
        InputManager.Instance.SetPlayerInputAction(true);
        foreach (var item in _usedItemIconList)
        {
            item.PushSelfToPool();
        }
        _usedItemIconList.Clear();
    }

    public override void OnShowFadePreComplete()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiShowPanelClip);

        Time.timeScale = 0f;
        //禁止玩家操作
        InputManager.Instance.SetPlayerInputAction(false);

        foreach (var (id, count) in InventoryManager.Instance.PackageData.PackageDict)
        {
            for(int i = 0; i < count; i++)
            {
                //设置背包格子
                PackagePanelItemIcon itemIcon = PoolManager.Instance.Pull<PackagePanelItemIcon>(_itemGridPrefab);
                itemIcon.SetParent(_container);

                //读取该道具的信息
                PackageItemInfo itemInfo = InventoryManager.Instance.GetItemInfoByID(id);
                itemIcon.Initialize(
                    id,
                    itemInfo.BonusEffectType,
                    itemInfo.BonusAmount,
                    itemInfo.Icon,
                    _toggleGroup,
                    (itemIcon) =>
                    {
                        _currentSelectedItem = itemIcon;
                        SetDescriptionText(itemInfo.Description);

                        if(itemIcon.ID == E_PackageItemID.Chen)
                        {
                            //晨露梦核不显示确定Button
                            _sureButton.gameObject.SetActive(false);
                        }
                        else
                        {
                            _sureButton.gameObject.SetActive(true);
                        }
                    });

                //添加到列表
                _usedItemIconList.Add(itemIcon);
            }
        }

        if(_usedItemIconList != null && _usedItemIconList.Count > 0)
        {
            //默认选中第一个
            _currentSelectedItem = _usedItemIconList[0];
            SetDescriptionText(InventoryManager.Instance.GetItemInfoByID(_currentSelectedItem.ID).Description);
        }
    }
}
