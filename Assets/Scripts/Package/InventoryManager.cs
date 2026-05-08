using System.Collections;
using System.Collections.Generic;
using Data.ScriptableObjects;
using UnityEngine;

public class InventoryManager : BaseManager<InventoryManager>
{
    public PackageData PackageData {  get; private set; }
    private PackageItemConfigSO _packageItemConfig;
    private InventoryManager() { }

    public void SetupData(PackageData packageData, PackageItemConfigSO m_packageItemConfig)
    {
        PackageData = packageData;
        this._packageItemConfig = m_packageItemConfig;
    }

    public void AddItemToPackage(EPackageItemType itemType)
    {
        if(PackageData.PackageDict.ContainsKey(itemType))
        {
            PackageData.PackageDict[itemType]++;
        }
        else
        {
            PackageData.PackageDict.Add(itemType, 1);
        }
    }
    public void RemoveItemFromPackage(EPackageItemType itemType)
    {
        if (PackageData.PackageDict.ContainsKey(itemType))
        {
            PackageData.PackageDict[itemType]--;
            if (PackageData.PackageDict[itemType] == 0)
            {
                PackageData.PackageDict.Remove(itemType);
            }
        }
    }
    public PackageItemInfo GetItemInfoByID(EPackageItemType type)
    {
        return _packageItemConfig.GetItemInfoByID(type);
    }
}
