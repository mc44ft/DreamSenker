using UnityEngine;

//使用方法：
//构造函数中先获取该数据类的随机密钥
//通过属性 在Set时加密 Get时解密
/// <summary>
/// 加密工具类
/// </summary>

public class EncryptionUtil
{
    /// <summary>
    /// 获取随机密钥
    /// </summary>
    /// <returns></returns>
    public static int GetRandomKey()
    {
        return Random.Range(1, 10000);
    }

    /// <summary>
    /// 加密数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int EncryptData(int data, int key)
    {
        int value = data ^ key;//异或加密
        value += key;
        return value;
    }

    public static long EncryptData(long data, int key)
    {
        long value = data ^ key;//异或加密
        value += key;
        return value;
    }

    public float EncryptData(float data, int key)
    {
        return data + key;//异或加密
    }

    /// <summary>
    /// 解密数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int DecryptData(int data, int key)
    {
        //有可能没有加密过，或者没有初始化的数据直接解密 就直接返回
        if (data == 0) return 0;
        int value = data - key;
        return value ^ key; ;
    }

    public static long DecryptData(long data, int key)
    {
        //有可能没有加密过，或者没有初始化的数据直接解密 就直接返回
        if (data == 0) return 0;
        long value = data - key;
        return value ^ key;
    }

    public static float DecryptData(float data, int key)
    {
        //有可能没有加密过，或者没有初始化的数据直接解密 就直接返回
        if (data == 0) return 0;
        return data - key; //异或解密
    }
}