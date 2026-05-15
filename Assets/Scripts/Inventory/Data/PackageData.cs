using System;
using System.Collections.Generic;
using DreamSeeker.Shared;

namespace DreamSeeker.Inventory.Data
{
    [Serializable]
    public class PackageData
    {
        //存储数据
        public List<ItemStackSaveData> Items = new List<ItemStackSaveData>();
        
        private Dictionary<EPackageItemType, int> _packageDict;
        //运行时构建字典数据
        public Dictionary<EPackageItemType, int> GetPackageDict()
        {
            if (_packageDict == null)
            {
                RebuildRuntimeDict();
            }
            return _packageDict;
        }

        private void RebuildRuntimeDict()
        {
            //为什么不用ToDictionary，因为这个方法要求Key不能重复，如果Key重复会报错，这里不太稳定
            // _packageDict = Items.ToDictionary(item => item.itemType, item => item.amount);
            
            if(_packageDict == null) _packageDict = new Dictionary<EPackageItemType, int>();
            if(Items == null)  Items = new List<ItemStackSaveData>();
            foreach (ItemStackSaveData item in Items)
            {
                //过滤脏数据
                if (item == null || item.Amount <= 0)
                {
                    continue;
                }
                _packageDict[item.ItemType] = item.Amount;
            }
        }
        
        //保存游戏数据前将字典重新刷入List
        public void SyncItemsFromRuntimeDict()
        {
            //字典为空，说明玩家本次游戏没有打开过背包，也就没机会重建字典，就不需要动Items
            if (_packageDict == null)
            {
                return;
            }
            Items ??= new List<ItemStackSaveData>();
            Items.Clear();
            foreach (KeyValuePair<EPackageItemType, int> pair in _packageDict)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }
                Items.Add(new ItemStackSaveData(pair.Key, pair.Value));
            }
        }
    }

    [Serializable]
    public class ItemStackSaveData
    {
        public EPackageItemType ItemType;
        public int Amount;

        //JsonUtility可能需要无参构造函数 这里留一个
        public ItemStackSaveData() { }

        public ItemStackSaveData(EPackageItemType itemType, int amount)
        {
            this.ItemType = itemType;
            this.Amount = amount;
        }
    }
}
