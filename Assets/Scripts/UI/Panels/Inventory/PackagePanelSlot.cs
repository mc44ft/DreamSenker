using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Inventory;
using DreamSeeker.Shared;

namespace DreamSeeker.UI.Panels.Inventory
{
[RequireComponent(typeof(Toggle))]
public class PackagePanelSlot : PoolBase
{
    [Tooltip("放缩大小")]
    [SerializeField] private float _punchStrength = 1.1f;
    [Tooltip("放缩时间")]
    [SerializeField] private float _punchDuration = 0.5f;
    [Tooltip("放缩曲线")]
    [SerializeField] private Ease _punchEase;
    [Tooltip("数量文本")]
    [SerializeField] private TextMeshProUGUI _countText;

    /// <summary>
    /// 当前格子绑定的物品类型
    /// </summary>
    public EPackageItemType ItemType => _stack.ItemType;
    /// <summary>
    /// 当前格子绑定的展示数据
    /// </summary>
    public InventoryItemStack Stack => _stack;

    private Toggle _toggle;
    private Image _image;
    private InventoryItemStack _stack;
    private Action<PackagePanelSlot> _onSelectedCallback;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// 初始化背包格子显示和选中回调
    /// </summary>
    public void Initialize(InventoryItemStack stack, ToggleGroup group, Action<PackagePanelSlot> onClickCallback)
    {
        _stack = stack;
        _image.sprite = stack.ItemInfo.Icon;
        _toggle.group = group;//设置组
        _onSelectedCallback = onClickCallback;
        SetCount(stack.Count);

        //使用对象池 清空之前的所有回调
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.SetIsOnWithoutNotify(false);
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    /// <summary>
    /// 设置当前格子的选中状态
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        _toggle.isOn = isSelected;
    }

    /// <summary>
    /// 设置数量文本
    /// </summary>
    private void SetCount(int count)
    {
        if (_countText == null)
        {
            return;
        }

        _countText.gameObject.SetActive(count > 1);
        _countText.text = count > 1 ? count.ToString() : string.Empty;
    }

    /// <summary>
    /// 响应 Toggle 选中状态变化
    /// </summary>
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)//当前Toggle被选中
        {
            transform.DOKill();
            transform.DOScale(_punchStrength, _punchDuration).SetEase(_punchEase);
            //通知外部更新状态
            _onSelectedCallback?.Invoke(this);
        }
        else
        {
            transform.DOKill();
            transform.DOScale(1f, _punchDuration).SetEase(_punchEase);
        }
    }

    public override void OnPull()
    {
        transform.localScale = Vector3.one;
    }

    public override void OnPush()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.SetIsOnWithoutNotify(false);
        _toggle.group = null;
        _image.sprite = null;
        _onSelectedCallback = null;
        _stack = default;
        SetCount(0);
    }
}
}
