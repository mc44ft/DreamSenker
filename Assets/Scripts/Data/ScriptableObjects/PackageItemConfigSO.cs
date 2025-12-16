using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[CreateAssetMenu(fileName = "PackageItemConfig_", menuName = "ScriptableObject/Config/PackageItemConfig")]
public class PackageItemConfigSO : ScriptableObject
{
    [field: SerializeField] public PackageItemInfo[] PackageItemInfoArray;
    public PackageItemInfo GetItemInfoByID(E_PackageItemID id)
    {
        return PackageItemInfoArray.Where(info => info.PackageItemID == id).FirstOrDefault();
    }
}
[Serializable]
public class PackageItemInfo
{
    public E_PackageItemID PackageItemID;
    public Sprite Icon;
    [Tooltip("物品描述")]
    [Multiline]
    public string Description;
    [Tooltip("物品加成效果")]
    public E_BonusEffectType BonusEffectType;
    [Tooltip("物品加成数值")]
    public int BonusAmount;
}