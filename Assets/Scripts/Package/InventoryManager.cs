using System.Collections;
using System.Collections.Generic;
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

    public void AddItemToPackage(E_PackageItemID itemID)
    {
        if(PackageData.PackageDict.ContainsKey(itemID))
        {
            PackageData.PackageDict[itemID]++;
        }
        else
        {
            PackageData.PackageDict.Add(itemID, 1);
        }
    }
    public void RemoveItemFromPackage(E_PackageItemID itemID)
    {
        if (PackageData.PackageDict.ContainsKey(itemID))
        {
            PackageData.PackageDict[itemID]--;
            if (PackageData.PackageDict[itemID] == 0)
            {
                PackageData.PackageDict.Remove(itemID);
            }
        }
    }
    public PackageItemInfo GetItemInfoByID(E_PackageItemID id)
    {
        return _packageItemConfig.GetItemInfoByID(id);
    }
}
