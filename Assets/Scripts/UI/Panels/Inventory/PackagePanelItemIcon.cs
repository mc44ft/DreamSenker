using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

using DreamSenker.Shared;

namespace DreamSenker.UI.Panels.Inventory
{
[RequireComponent(typeof(Toggle))]
public class PackagePanelItemIcon : PoolBase
{
    [Tooltip("放缩大小")]
    [SerializeField] private float _punchStrength = 1.1f;
    [Tooltip("放缩时间")]
    [SerializeField] private float _punchDuration = 0.5f;
    [Tooltip("放缩曲线")]
    [SerializeField] private Ease _punchEase;

    public EPackageItemType packageItemType;
     public EBonusEffectType BonusEffectType;
    public int BonusAmount;

    private Toggle _toggle;
    private Image _image;
    
    private Action<PackagePanelItemIcon> _onSelectedCallback;
    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        _image = GetComponent<Image>();
    }
    public void Initialize(EPackageItemType itemType, EBonusEffectType BonusEffectType, int bonusAmount, 
        Sprite sprite, ToggleGroup group, Action<PackagePanelItemIcon> onClickCallback)
    {
        packageItemType = itemType;
        this.BonusEffectType = BonusEffectType;
        BonusAmount = bonusAmount;
        _image.sprite = sprite;
        _toggle.group = group;//设置组
        _onSelectedCallback = onClickCallback;

        //使用对象池 清空之前的所有回调
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)//当前Toggle被选中
        {
            transform.DOScale(_punchStrength, _punchDuration).SetEase(_punchEase);
            //通知外部更新状态
            _onSelectedCallback?.Invoke(this);
        }
        else
        {
            transform.DOScale(1f, _punchDuration).SetEase(_punchEase);
        }
    }

    public override void OnPull()
    {
        
    }

    public override void OnPush()
    {
        
    }
}
}
