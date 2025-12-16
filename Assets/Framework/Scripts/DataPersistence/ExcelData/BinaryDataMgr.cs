using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class BinaryDataMgr : BaseManager<BinaryDataMgr>
{
    /// <summary>
    /// 用于存储所有普通Excel表数据的容器
    /// key为容器类名
    /// value为容器类
    /// </summary>
    private Dictionary<string, object> _containerDict = new Dictionary<string, object>();
    /// <summary>
    /// 所有Excel表二进制数据存放的路径（对话系统二进制数据也在其中）
    /// </summary>
    public static string EXCEL_BINARY_DATA_SAVE_PATH = Path.Combine(Application.streamingAssetsPath, "BinaryData");
    /// <summary>
    /// 可读写数据保存的路径
    /// </summary>
    public static string SAVE_PATH = Path.Combine(Application.persistentDataPath, "Data");

    private BinaryDataMgr() 
    {
        InitData();
    }

    /// <summary>
    /// Excel表数据可以一开始就初始化
    /// </summary>
    private void InitData()
    {
        
    }

    /// <summary>
    /// 加载指定类型的二进制数据
    /// </summary>
    /// <typeparam name="T">容器类</typeparam>
    /// <typeparam name="K">数据类</typeparam>
    public void LoadBinary<T, K>(string excelName)
    {
        //将数据类名的最后四个字符（Data）去除 作为二进制文件名称
        string fileName = typeof(K).Name;
        fileName = fileName.Substring(0, fileName.Length - 4);

        string openFilePath = Path.Combine(EXCEL_BINARY_DATA_SAVE_PATH, excelName, fileName + ".cao");

        //读取Excel对应的二进制数据文件
        using (FileStream fs = File.Open(openFilePath, FileMode.Open, FileAccess.Read))
        {
            //声明字节数组容器
            byte[] bytes = new byte[fs.Length];
            //读取单张Excel表的全部数据
            fs.Read(bytes, 0, bytes.Length);
            //关闭文件流
            fs.Close();

            //读取索引进度
            int index = 0;
            //读取总行数
            int rowCount = BitConverter.ToInt32(bytes, index);
            index += 4;
            //读取总列数
            int columnCount = BitConverter.ToInt32(bytes, index);
            index += 4;
            //读取主键变量名字节长度
            int keyNameLength = BitConverter.ToInt32(bytes, index);
            index += 4;
            //读取主键变量名
            string keyName = Encoding.UTF8.GetString(bytes, index, keyNameLength);
            index += keyNameLength;
            //创建容器类对象
            Type contaninerType = typeof(T);
            object contaninerObj = Activator.CreateInstance(contaninerType);
            //获取容器类的字典
            object dicObject = contaninerType.GetField("dataDict").GetValue(contaninerObj);
            //获取容器类中字典的Add方法
            MethodInfo methodInfo = dicObject.GetType().GetMethod("Add");

            Type classType = typeof(K);
            //通过反射得到的字段顺序是不准确的 这里改用字段名获取字段信息
            //FieldInfo[] infos = classType.GetFields();//通过反射得到数据类的所有字段信息
            for (int i = 0; i < rowCount; i++)
            {
                //快速实例化一个数据类对象
                object classObj = Activator.CreateInstance(classType);
                for (int j = 0; j < columnCount; j++)
                {
                    //这里的改动是将字段名也作为数据存储在了二进制中
                    //读取字段名
                    int nameLength = BitConverter.ToInt32(bytes, index);
                    index += 4;
                    string name = Encoding.UTF8.GetString(bytes, index, nameLength);
                    index += nameLength;

                    //通过字段名获取反射信息
                    FieldInfo info = classType.GetField(name);
                    //填充字段值
                    SetFieldInfoValue(classObj, info, bytes, ref index, classType);
                }
                //获取主键字段的值
                object keyValue = classType.GetField(keyName).GetValue(classObj);
                //将数据类对象添加到容器类对象的字典中
                methodInfo.Invoke(dicObject, new object[] { keyValue, classObj });
            }
            //字典的键为数据容器的类名
            _containerDict.Add(typeof(T).Name, contaninerObj);//记录表（容器）信息
        }
    }
    private void SetFieldInfoValue(object classObj, FieldInfo info, byte[] bytes, ref int index, Type classType)
    {
        if (info.FieldType == typeof(int))
        {
            info.SetValue(classObj, BitConverter.ToInt32(bytes, index));//设置每个字段的值
            index += 4;//移动索引
        }
        else if (info.FieldType == typeof(float))
        {
            info.SetValue(classObj, BitConverter.ToSingle(bytes, index));
            index += 4;
        }
        else if (info.FieldType == typeof(bool))
        {
            info.SetValue(classObj, BitConverter.ToBoolean(bytes, index));
            index += 1;
        }
        else if (info.FieldType == typeof(string))
        {
            int length = BitConverter.ToInt32(bytes, index);//获取字符串长度
            index += 4;
            string str = Encoding.UTF8.GetString(bytes, index, length);//获取字符串内容
            index += length;
            info.SetValue(classObj, str);
        }
        else
        {
            Debug.LogError("未知类型：" + info.FieldType);
            Debug.LogError(classType.Name);
        }
    }
    /// <summary>
    /// 得到指定表的信息
    /// </summary>
    /// <typeparam name="T">容器类</typeparam>
    /// <returns></returns>
    public T GetContainer<T>() where T : class
    {
        string key = typeof(T).Name;
        return _containerDict.ContainsKey(key) ? _containerDict[key] as T : null;
    }
    #region 可读写数据
    public void Save(string fileName, object data)
    {
        if (!Directory.Exists(SAVE_PATH))
        {
            Directory.CreateDirectory(SAVE_PATH);
        }
        string path = Path.Combine(SAVE_PATH, fileName + ".cao");
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, data);
            fs.Flush();
        }
    }

    public T Read<T>(string fileName) where T : class
    {
        T data;
        string path = Path.Combine(SAVE_PATH, fileName + ".cao");
        //先判断默认文件夹下是否有该文件 没有再读取初始文件夹中的文件
        if (!File.Exists(path))
            path = Path.Combine(EXCEL_BINARY_DATA_SAVE_PATH, fileName + ".cao");
        if (!File.Exists(path))
        {
            Debug.LogWarning("未找到该二进制数据文件！");
            return default(T);//返回默认值
        }
        using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
        {
            BinaryFormatter bf = new BinaryFormatter();
            data = (T)bf.Deserialize(fs);
        }
        return data;
    }
    #endregion

}