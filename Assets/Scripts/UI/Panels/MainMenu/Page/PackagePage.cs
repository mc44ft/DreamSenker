using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Inventory;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.UI.Panels.Inventory
{
public class PackagePage : MonoBehaviour
{
    [Header("GRID")]
    [SerializeField] private GameObject _itemGridPrefab;
    [SerializeField] private Transform _container;

    [Header("UI COMPONENT")]
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Toggle _usableTabToggle;
    [SerializeField] private Toggle _questTabToggle;
    [SerializeField] private TextMeshProUGUI _contentText;


    /// <summary>
    /// 当前面板使用中的格子实例
    /// </summary>
    private readonly List<PackagePanelSlot> _usedItemSlotList = new List<PackagePanelSlot>();
    /// <summary>
    /// 当前选中的背包格子
    /// </summary>
    private PackagePanelSlot _currentSelectedSlot;
    /// <summary>
    /// 当前打开的背包标签页
    /// </summary>
    private EPackageItemCategory _currentCategory = EPackageItemCategory.Usable;

    private void Start()
    {
        if (_usableTabToggle != null)
            _usableTabToggle.onValueChanged.AddListener(OnUsableTabValueChanged);
        if (_questTabToggle != null)
            _questTabToggle.onValueChanged.AddListener(OnQuestTabValueChanged);
    }
    private void OnDestroy()
    {
        if (_usableTabToggle != null)
            _usableTabToggle.onValueChanged.RemoveListener(OnUsableTabValueChanged);
        if (_questTabToggle != null)
            _questTabToggle.onValueChanged.RemoveListener(OnQuestTabValueChanged);
    }

    /// <summary>
    /// 切换到道具标签页
    /// </summary>
    private void OnUsableTabValueChanged(bool isOn)
    {
        if (isOn)
        {
            SwitchCategory(EPackageItemCategory.Usable);
        }
    }

    /// <summary>
    /// 切换到任务物品标签页
    /// </summary>
    private void OnQuestTabValueChanged(bool isOn)
    {
        if (isOn)
        {
            SwitchCategory(EPackageItemCategory.Quest);
        }
    }

    /// <summary>
    /// 切换背包标签页
    /// </summary>
    public void SwitchCategory(EPackageItemCategory category)
    {
        _currentCategory = category;
        SyncCategoryToggles();
        Refresh();
    }

    /// <summary>
    /// 刷新当前标签页下的背包格子
    /// </summary>
    public void Refresh()
    {
        RecycleSlots();
        _currentSelectedSlot = null;

        List<InventoryItemStack> stacks = InventoryManager.Instance.GetItemStacks(_currentCategory);
        foreach (InventoryItemStack stack in stacks)
        {
            // 设置背包格子
            PackagePanelSlot slot = PoolManager.Instance.Pull<PackagePanelSlot>(_itemGridPrefab);
            slot.SetParent(_container);
            slot.Initialize(stack, _toggleGroup, SelectSlot);
            _usedItemSlotList.Add(slot);
        }

        if (_usedItemSlotList.Count <= 0)
        {
            SetDescriptionText(_currentCategory == EPackageItemCategory.Quest ? "没有任务物品" : "你的背包空空如也");
        }
        else
        {
            SetDescriptionText(_currentCategory == EPackageItemCategory.Quest ? "单击查看" : "单击查看 双击使用");
        }
    }

    /// <summary>
    /// 选中指定背包格子并刷新描述
    /// </summary>
    private void SelectSlot(PackagePanelSlot slot)
    {
        _currentSelectedSlot = slot;
        SetDescriptionText(slot.Stack.ItemInfo.Description);
    }

    /// <summary>
    /// 回收当前显示的所有格子
    /// </summary>
    private void RecycleSlots()
    {
        foreach (PackagePanelSlot slot in _usedItemSlotList)
        {
            slot.PushSelfToPool();
        }
        _usedItemSlotList.Clear();
    }

    /// <summary>
    /// 同步标签页 Toggle 状态
    /// </summary>
    private void SyncCategoryToggles()
    {
        if (_usableTabToggle != null)
        {
            _usableTabToggle.SetIsOnWithoutNotify(_currentCategory == EPackageItemCategory.Usable);
        }
        if (_questTabToggle != null)
        {
            _questTabToggle.SetIsOnWithoutNotify(_currentCategory == EPackageItemCategory.Quest);
        }
    }

    /// <summary>
    /// 设置描述文本
    /// </summary>
    private void SetDescriptionText(string content)
    {
        _contentText.text = content;
    }

    private void OnEnable()
    {
        _currentCategory = EPackageItemCategory.Usable;
        SyncCategoryToggles();
        Refresh();
    }

    private void OnDisable()
    {
        RecycleSlots();
    }
}
}
