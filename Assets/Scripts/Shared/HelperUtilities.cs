using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamSeeker.UI;

namespace DreamSeeker.Shared
{
public static class HelperUtilities
{
    /// <summary>
    /// 击退
    /// </summary>
    public static void DoKnockback(Rigidbody2D rigidbody, Vector2 attackDirection, float addForceValue)
    {
        Vector2 kockbackDirection = new Vector2(attackDirection.x, 1).normalized;
        rigidbody.velocity = Vector2.zero;
        rigidbody.AddForce(kockbackDirection * addForceValue, ForceMode2D.Impulse);
    }

    /// <summary>
    /// 显示提示弹窗面板。
    /// </summary>
    /// <param name="position">提示在 Canvas 上的垂直位置。</param>
    /// <param name="message">要显示的提示信息。</param>
    public static void ShowTipPopup(TipPopupPosition position, string message)
    {
        TipPopupPanel.ShowTip(position, message);
    }

    /// <summary>
    /// 用于给容器洗牌的方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        //不只是List 数组也继承了IList接口 所以这个方法也可以给数组使用
        //this是拓展方法

        for(int i = list.Count -1; i > 0; i--)
        {
            //这样写是为了当前遍历的元素跟剩余的随机元素交换
            int j = Random.Range(0, i + 1);

            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
}
