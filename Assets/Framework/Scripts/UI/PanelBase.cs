using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 自动化版本
/// 使用泛型使得每个子类都有自己特定的枚举类型
/// 继承该基类的面板类都必须在外部声明一个独属于自己的枚举类型
/// 枚举命名格式 E_面板类型_ControlName
/// 该枚举统计需要缓存的组件名称 不在此枚举中的组件不会缓存
/// 同一面板下需要使用的组件名称不能重名
/// 所以统一命名格式为UI_xxx
/// 
/// 小项目中 使用手动化版本 面板类只需继承IPanel接口即可
/// 手动进行控件拖拽和事件监听
/// </summary>
/// <typeparam name="E"></typeparam>
public abstract class PanelBase<E> : MonoBehaviour, IPanel where E : struct, Enum
{
    /// <summary>
    /// 缓存所有用到的UI控件
    /// </summary>
    protected Dictionary<E, UIBehaviour> _controlDict = new Dictionary<E, UIBehaviour>();

    protected virtual void Awake()
    {
        //一个对象上可能存在两种及以上的控件
        //因为是根据脚本查找 如果一个对象上有多个控件
        //第二次查找就会重复查找到第一次查找到的对象
        //我们只存储第一次查找到的组件
        //要使用该对象上的其他组件
        //点出来使用就好了

        //优先查找重要的控件
        FindChildrenControls<Button>();
        FindChildrenControls<Toggle>();
        FindChildrenControls<Slider>();
        FindChildrenControls<InputField>();
        FindChildrenControls<ScrollRect>();
        FindChildrenControls<Dropdown>();
        //次要查找的控件
        FindChildrenControls<Text>();
        FindChildrenControls<TextMeshProUGUI>();
        FindChildrenControls<Image>();
    }
    private void FindChildrenControls<T>() where T : UIBehaviour
    {
        //参数一：是否同时寻找失活控件
        T[] controls = GetComponentsInChildren<T>(true);

        for (int i = 0; i < controls.Length; i++)
        {
            //为了使用闭包 将该变量声明到for循环内部
            E controlName;
            //如果该组件存在于枚举中
            //解析成功返回true 否则返回false
            bool isSuccess = Enum.TryParse(controls[i].gameObject.name, out controlName);
            if (isSuccess)
            {
                //组件不能重名
                //一个组件名对应一个字典的键值
                if (!_controlDict.ContainsKey(controlName))
                {
                    _controlDict.Add(controlName, controls[i]);
                    //判断控件类型 决定是否需要添加监听事件
                    if (controls[i] is Button button)
                    {
                        button.onClick.AddListener(() => { ClickButton(controlName); });//闭包
                    }
                    else if (controls[i] is Toggle toggle)
                    {
                        toggle.onValueChanged.AddListener((value) => { ChangeValueToggle(controlName, value); });
                    }
                    else if (controls[i] is Slider slider)
                    {
                        slider.onValueChanged.AddListener((value) => { ChangeValueSlider(controlName, value); });
                    }
                }
            }

        }
    }
    /// <summary>
    /// 获取控件
    /// </summary>
    /// <typeparam name="T">控件类型</typeparam>
    /// <param name="name">控件名称</param>
    /// <returns></returns>
    public T GetControl<T>(E name) where T : UIBehaviour
    {
        if (_controlDict.ContainsKey(name))
        {
            T control = _controlDict[name] as T;
            //举例：如果存的是Button，查找的是Button上的Image 就会转换错误
            if (control == null)
            {
                Debug.LogError($"控件{name}类型转换失败，请检查控件类型是否正确");
                return null;
            }
            return _controlDict[name] as T;
        }
        else
        {
            Debug.LogError($"控件{name}不存在");
            return null;
        }
    }

    /// <summary>
    /// 点击按钮事件
    /// </summary>
    /// <param name="name">通过这个名称来判断是哪个Button被点击了</param>
    protected virtual void ClickButton(E name) { }

    /// <summary>
    /// 切换Toggle事件
    /// </summary>
    /// <param name="name">通过这个名称来判断是哪个Toggle被点击了</param>
    /// <param name="value"></param>
    protected virtual void ChangeValueToggle(E name, bool value) { }

    /// <summary>
    /// 滑动Slider事件
    /// </summary>
    /// <param name="name">通过这个名称来判断是哪个Slider被触发了</param>
    /// <param name="value"></param>
    protected virtual void ChangeValueSlider(E name, float value) { }

    public abstract void OnShowFadePreComplete();

    public abstract void OnHideFadedComplete();
}