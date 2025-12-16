using System;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 字符串处理的工具类
/// </summary>
public class TextUtil
{
    #region 字符串拆分

    /// <summary>
    ///我们在配置表中
    ///经常会配置如下格式的字符串数据
    ///1,10;2,4;32,1
    ///2,11|3,12|4,55
    ///遇到这种数据时，我们往往会使用string中的Split方法进行拆分
    ///我们想要通过封装满足以下的需求
    ///1.避免符号填写错误，英文字符填写成中文字符
    ///2.能够直接拆分获取到int数组
    /// </summary>
    /// <param name="str">被拆分的字符串</param>
    /// <param name="type">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <returns></returns>
    public static string[] SplitStr(string str, int type = 1)
    {
        //不直接返回null 是为了避免外部直接遍历数组出现报错
        if (str == "") return new string[0];//返回一个空数组
        string s = str;
        if (type == 1)//;
        {
            while (s.IndexOf("；") != -1)//如果有中文分号
                s = s.Replace("；", ";");//将中文分号替换为对应的英文符号
            return s.Split(';');
        }
        else if (type == 2)//,
        {
            while (s.IndexOf("，") != -1)//如果有中文分号
                s = s.Replace("，", ",");//将中文分号替换为对应的英文符号
            return s.Split(',');
        }
        else if (type == 3)//%
        {
            return s.Split('%');
        }
        else if (type == 4)//:
        {
            while (s.IndexOf("：") != -1)//如果有中文分号
                s = s.Replace("：", ":");//将中文分号替换为对应的英文符号
            return s.Split(':');
        }
        else if (type == 5)//
        {
            return s.Split(' ');
        }
        else if (type == 6)//|
        {
            return s.Split('|');
        }
        else if (type == 7)//_
        {
            return s.Split('_');
        }
        else if (type == 8)//-
        {
            return s.Split('-');
        }
        else
        {
            Debug.LogError("不支持的符号类型");
            return null;
        }
    }

    /// <summary>
    /// 拆分为int数组
    /// </summary>
    /// <param name="str">被拆分的字符串</param>
    /// <param name="type">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <returns></returns>
    public static int[] SplitStrToIntArray(string str, int type = 1)
    {
        string[] strs = SplitStr(str, type);//得到拆分后的字符串数组
        if (strs.Length == 0)
            return new int[0];
        //将字符串数组转换为int数组
        return Array.ConvertAll<string, int>(strs, (str) =>
        {
            return int.Parse(str);
        });
    }

    /// <summary>
    /// int键值对
    /// 拆分组合信息的方法 比如 id1,数量1:id2,数量2 这种格式的字符串
    /// </summary>
    /// <param name="str">被拆分的字符串</param>
    /// <param name="typeOne">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <param name="typeTwo">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <param name="callback"></param>
    public static void SplitStrToCallback(string str, int typeOne, int typeTwo, UnityAction<int, int> callback)
    {
        string[] strs = SplitStr(str, typeOne);
        int[] ints;
        foreach (string s in strs)
        {
            ints = SplitStrToIntArray(s, typeTwo);
            if (ints.Length == 0)
                continue;
            callback?.Invoke(ints[0], ints[1]);
        }
    }

    /// <summary>
    /// string键值对
    /// </summary>
    /// <param name="str">被拆分的字符串</param>
    /// <param name="typeOne">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <param name="typeTwo">拆分字符类型：1-";"，2-","，3-"%"，4-":"，5-" "，6-"|"，7-"_"，8-"-"</param>
    /// <param name="callback"></param>
    public static void SplitStrToCallback(string str, int typeOne, int typeTwo, UnityAction<string, string> callback)
    {
        string[] strs = SplitStr(str, typeOne);
        string[] ints;
        foreach (string s in strs)
        {
            ints = SplitStr(s, typeTwo);
            if (ints.Length == 0)
                continue;
            callback?.Invoke(ints[0], ints[1]);
        }
    }

    #endregion 字符串拆分

    #region 保留

    /// <summary>
    /// 转字符串数字前补0
    /// </summary>
    /// <param name="num"></param>
    /// <param name="length">补零长度</param>
    /// <returns></returns>
    public static string ToStringWithZero(int num, int length)
    {
        //string str = num.ToString();
        //for(int i = str.Length - 1; i < length; i++)
        //{
        //    str = $"0{str}";
        //}
        //return str;
        //D表示十进制数字，大括号内表示长度
        //如果num的长度小于length，则会在前面补0
        return num.ToString($"D{length}");
    }

    /// <summary>
    /// 转字符串保留n位小数
    /// </summary>
    /// <param name="num"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string ToStringWithDecimal(float num, int length)
    {
        //F表示浮点数，大括号内表示保留小数的位数
        //不是四舍五入 而是直接截取
        return num.ToString($"F{length}");
    }

    #endregion 保留

    #region 时间转换

    private static StringBuilder result = new StringBuilder("");

    /// <summary>
    /// 秒转时分秒
    /// </summary>
    /// <param name="allSecond">总时间（秒）</param>
    /// <param name="ignoreZero">是否忽略 时、分位 的零</param>
    /// <param name="hourStr">小时显示符号</param>
    /// <param name="minuteStr">分钟显示符号</param>
    /// <param name="secondStr">秒数显示符号</param>
    /// <returns></returns>
    public static string SecondsToHMS(int allSecond, bool ignoreZero = false, string hourStr = "时", string minuteStr = "分", string secondStr = "秒")
    {
        //时间不为负数
        if (allSecond < 0) allSecond = 0;
        int hour = allSecond / 3600;//计算小时
        int second = allSecond % 3600;//计算剩余秒数
        int minute = second / 60;//计算分钟
        second = allSecond % 60;//计算秒数
        result.Clear();
        if (!ignoreZero || hour != 0)
        {
            result.Append(hour + hourStr);
        }
        //即便忽略零 分钟也为零 但是小时不为零 分钟就必须显示
        if (!ignoreZero || minute != 0 || hour != 0)
        {
            result.Append(minute + minuteStr);
        }
        result.Append(second + secondStr);
        return result.ToString();
    }

    /// <summary>
    /// 00:00:00
    /// </summary>
    /// <param name="allSecond">总时间（秒）</param>
    /// <param name="separator">分隔符</param>
    /// <returns></returns>
    public static string SecondsToTime(int allSecond, bool ignoreZero = false, string separator = ":")
    {
        if (allSecond < 0) allSecond = 0;
        int hour = allSecond / 3600;//计算小时
        int second = allSecond % 3600;//计算剩余秒数
        int minute = second / 60;//计算分钟
        second = allSecond % 60;//计算秒数
        result.Clear();
        if (!ignoreZero || hour != 0)
        {
            result.Append(ToStringWithZero(hour, 2) + separator);
        }
        //即便忽略零 分钟也为零 但是小时不为零 分钟就必须显示
        if (!ignoreZero || minute != 0 || hour != 0)
        {
            result.Append(ToStringWithZero(minute, 2) + separator);
        }
        result.Append(ToStringWithZero(second, 2));
        return result.ToString();
    }

    #endregion 时间转换

    #region 大数据转换

    /// <summary>
    /// 用汉字字符格式化数据
    /// </summary>
    /// <param name="num"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string FormatNumberWithWord(int num, int length = 1)
    {
        if (num < 10000)
        {
            return num.ToString();
        }
        else if (num < 100000000)
        {
            //format参数
            //保留一位小数(Fixed - Point "F1"):
            //小数部分会进行四舍五入
            return (num / 10000f).ToString("F" + length) + "万";
        }
        else
        {
            //亿
            return (num / 100000000f).ToString("F" + length) + "亿";
        }
    }

    /// <summary>
    /// 用逗号来千分位格式化数据
    /// </summary>
    /// <param name="num"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string FormatNumberWithCommas(int num)
    {
        return num.ToString("N0");
    }

    #endregion 大数据转换
}