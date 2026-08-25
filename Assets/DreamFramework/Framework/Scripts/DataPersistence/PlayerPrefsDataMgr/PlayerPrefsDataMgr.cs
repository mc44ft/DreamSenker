using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

/// <summary>
/// playerPrefs数据管理类
/// </summary>
public class PlayerPrefsDataMgr
{
    private static PlayerPrefsDataMgr _instance = new PlayerPrefsDataMgr();

    private PlayerPrefsDataMgr()
    {
    }

    public static PlayerPrefsDataMgr Instance
    {
        get
        {
            return _instance;
        }
    }

    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="data">数据对象</param>
    /// <param name="keyName">数据对象的唯一key</param>
    public void SaveData(object data, string keyName)
    {
        //通过type得到传入数据对象的所有字段
        //结合PlayerPrefs 来进行存储

        //得到传入数据对象的Type
        Type type = data.GetType();
        //得到该type的所有字段（公共成员变量）
        FieldInfo[] fieldInfos = type.GetFields();

        #region KeyName的规则

        //keyName_数据类型_字段类型_字段名

        #endregion KeyName的规则

        string saveKeyName;
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            //每次进入都进行拼接
            saveKeyName = keyName + "_" + type.Name + "_" + fieldInfos[i].FieldType.Name + "_" + fieldInfos[i].Name;
            SaveValue(fieldInfos[i].GetValue(data), saveKeyName);
        }
        //写入硬盘
        PlayerPrefs.Save();
    }

    private void SaveValue(object value, string saveKeyName)
    {
        //分类存储
        if (value.GetType() == typeof(int))
        {
            //为int数据加密
            int rValue = (int)value;
            rValue += 10;
            PlayerPrefs.SetInt(saveKeyName, rValue);
        }
        else if (value.GetType() == typeof(float))
        {
            PlayerPrefs.SetFloat(saveKeyName, (float)value);
        }
        else if (value.GetType() == typeof(string))
        {
            PlayerPrefs.SetString(saveKeyName, (string)value);
        }
        else if (value.GetType() == typeof(bool))
        {
            PlayerPrefs.SetInt(saveKeyName, (bool)value ? 1 : 0);
        }
        else if (typeof(IList).IsAssignableFrom(value.GetType()))
        {
            //因为不知道list泛型是什么所以用 IList装
            IList list = value as IList;
            PlayerPrefs.SetInt(saveKeyName, list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                //递归
                SaveValue(list[i], saveKeyName + i);
            }
        }
        else if (typeof(IDictionary).IsAssignableFrom(value.GetType()))
        {
            IDictionary dic = value as IDictionary;
            PlayerPrefs.SetInt(saveKeyName, dic.Count);
            int index = 0;
            foreach (object key in dic.Keys)
            {
                SaveValue(key, saveKeyName + "_key_" + index);
                SaveValue(dic[key], saveKeyName + "_value_" + index);
                index++;
            }
        }
        //基础数据类型都不是 就是自定义数据类型
        else
        {
            SaveData(value, saveKeyName);
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="type">想要读取的数据类型</param>
    /// <param name="keyName">唯一key</param>
    /// <returns></returns>
    public object LoadData(Type type, string keyName)
    {
        //传入Type 返回这个类型的对象 省去了外部去实例化一个对象的操作 节约了代码量

        //根据传入的type 和keyName
        //依据key的拼接规则 来进行数据的获取赋值 并将实例化后的对象返回出去

        //快速实例化一个用于接收数据的对象
        object data = Activator.CreateInstance(type);
        //得到该type的所有公共成员变量信息
        FieldInfo[] fieldInfos = type.GetFields();
        string loadKeyName;
        //直接读取
        for (int i = 0; i < fieldInfos.Length; i++)
        {
            loadKeyName = keyName + "_" + type.Name + "_" + fieldInfos[i].FieldType.Name + "_" + fieldInfos[i].Name;
            fieldInfos[i].SetValue(data, LoadValue(fieldInfos[i].FieldType, loadKeyName));
        }
        return data;
    }

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="data">读取数据的对象</param>
    /// <param name="loadKeyName">唯一key</param>
    private object LoadValue(Type type, string loadKeyName)
    {
        //分类读取
        if (type == typeof(int))
        {
            //为int数据解密
            return PlayerPrefs.GetInt(loadKeyName) - 10;
        }
        else if (type == typeof(float))
        {
            return PlayerPrefs.GetFloat(loadKeyName);
        }
        else if (type == typeof(string))
        {
            return PlayerPrefs.GetString(loadKeyName);
        }
        else if (type == typeof(bool))
        {
            return PlayerPrefs.GetInt(loadKeyName) == 1 ? true : false;
        }
        else if (typeof(IList).IsAssignableFrom(type))
        {
            IList list = Activator.CreateInstance(type) as IList;
            //得到list泛型类型
            Type[] types = type.GetGenericArguments();
            //得到list容器长度
            int list_Count = PlayerPrefs.GetInt(loadKeyName, 0);
            for (int i = 0; i < list_Count; i++)
            {
                //递归
                list.Add(LoadValue(types[0], loadKeyName + i));
            }
            return list;
        }
        else if (typeof(IDictionary).IsAssignableFrom(type))
        {
            IDictionary dic = Activator.CreateInstance(type) as IDictionary;
            int dic_COunt = PlayerPrefs.GetInt(loadKeyName, 0);
            //获取泛型类型
            Type[] types = type.GetGenericArguments();
            for (int i = 0; i < dic_COunt; i++)
            {
                dic.Add(LoadValue(types[0], loadKeyName + "_key_" + i),
                        LoadValue(types[1], loadKeyName + "_value_" + i));
            }
            return dic;
        }
        //基础数据类型都不是 就是自定义数据类型
        else
        {
            return LoadData(type, loadKeyName);
        }
    }
}