using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;

public enum E_UILayer
{
    //----------------Overlay Canvas 下的层级选项----------------
    Botton, //底层
    Middle, //中间层
    Top,    //顶层
    System,  //系统层
    //----------------Camera Canvas 下的层级选项----------------
    SpecialLow,
    SpecialHigh,
}

/// <summary>
/// Canvas和EventSyatem预制体一般放在Resources/UI下
/// 如果是Overlay模式 不需要考虑摄像机
/// 如果是Camera模式 需要将UI摄像机也制作为预制体 并放在Resources/UI下
/// 因为目前是懒加载模式 一下子创建很多UI可能会导致卡顿 可以现在GameManager里预加载一下
/// 面板预制体一般放在AB包中
/// 面板预制体名与其类名一致
/// </summary>
public class UIManager : BaseManager<UIManager>
{
    #region 内部类

    private abstract class PanelInfoBase
    {
        /// <summary>
        /// 该参数标记 是否在面板未加载成功之前就将面板Hide  如果是这样 直接从字典中移除面板即可
        /// </summary>
        public bool IsLoadPreHide = false;
        /// <summary>
        /// 是否处于显示状态
        /// </summary>
        public bool IsDisplaying = false;
    }

    private class PanelInfo<T> : PanelInfoBase where T : MonoBehaviour, IPanel
    {
        public T Panel = null;

        /// <summary>
        /// Fade前的回调
        /// </summary>
        public Action<T> OnShowPanelFadePre;
        /// <summary>
        /// Fade后的回调
        /// </summary>
        public Action<T> OnShowPanelFaded;
        public Widget Widget;
        
    }

    #endregion 内部类
    //Overlay模式 UI全权交给Canvas绘制 会显示在一切摄像机渲染物体的前面 不需要设置相机
    public Canvas OverlayCanvas { get; private set; }
    public Canvas CameraCanvas { get; private set; }
    public Camera UICamera { get; private set; }
    private EventSystem m_eventSystem;

    //基础层
    private Transform m_bottonLayer;
    private Transform m_middleLayer;
    private Transform m_topLayer;
    private Transform m_systemLayer;
    //CameraCanvas下的特殊层级
    private Transform m_specialLow;
    private Transform m_specialHigh;

    //所有面板对象
    private Dictionary<string, PanelInfoBase> _panelDict = new Dictionary<string, PanelInfoBase>();

    private UIManager()
    {
        //将所有UI对象都放在指定的根对象下
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
        }

        //同步加载（因为马上就要用）UI摄像机、Canvas和EventSystem预制体
        OverlayCanvas = GameObject.Instantiate(GameResourcesSystem.Instance.OverlayCanvas, uiRoot.transform).GetComponent<Canvas>();
        CameraCanvas = GameObject.Instantiate(GameResourcesSystem.Instance.CameraCanvas, uiRoot.transform).GetComponent<Canvas>();
        m_eventSystem = GameObject.Instantiate(GameResourcesSystem.Instance.EventSystem, uiRoot.transform).GetComponent<EventSystem>();
        UICamera = GameObject.Instantiate(GameResourcesSystem.Instance.UiCamera, uiRoot.transform).GetComponent<Camera>();

        //为CameraCanvas加载并设置UI摄像机
        CameraCanvas.worldCamera = UICamera;
        //若为URP项目 则主摄像机为base相机
        //配置摄像机堆栈
        SetCameraStack();


        //更名
        OverlayCanvas.gameObject.name = "OverlayCanvas";
        CameraCanvas.gameObject.name = "CameraCanvas";
        UICamera.gameObject.name = "UICamera";
        m_eventSystem.gameObject.name = "EventSystem";

        //过场景不移除
        GameObject.DontDestroyOnLoad(uiRoot);

        //在预制体下寻找层级
        //只寻找第一层子级
        m_bottonLayer = OverlayCanvas.transform.Find("Botton");
        m_middleLayer = OverlayCanvas.transform.Find("Middle");
        m_topLayer = OverlayCanvas.transform.Find("Top");
        m_systemLayer = OverlayCanvas.transform.Find("System");
        m_specialLow = CameraCanvas.transform.Find("SpecialLow");
        m_specialHigh = CameraCanvas.transform.Find("SpecialHigh");
    }
    public void SetCameraStack()
    {
        var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
        //如果获取成功 且摄像机是一个base摄像机
        if (mainCameraData != null && mainCameraData.renderType == CameraRenderType.Base)
        {
            var uiCameraData = UICamera.GetUniversalAdditionalCameraData();
            //uiCamera必须为覆盖模式
            if (uiCameraData != null && uiCameraData.renderType == CameraRenderType.Overlay)
            {
                //检查该ui摄像机未被添加过
                if (!mainCameraData.cameraStack.Contains(UICamera))
                {
                    mainCameraData.cameraStack.Add(UICamera);
                }
            }

        }
    }
    /// <summary>
    /// 因为构造函数中实例化和Find了很多对象，所有一瞬间加载会导致卡顿 所有用此方法来预加载一下
    /// </summary>
    public void PreLoad() { }
    /// <summary>
    /// 获取基础层级Transform
    /// </summary>
    /// <param name="layer">层级枚举</param>
    /// <returns></returns>
    public Transform GetBaseLayer(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Botton:
                return m_bottonLayer;
            case E_UILayer.Middle:
                return m_middleLayer;
            case E_UILayer.Top:
                return m_topLayer;
            case E_UILayer.System:
                return m_systemLayer;
            case E_UILayer.SpecialLow:
                return m_specialLow;
            case E_UILayer.SpecialHigh:
                return m_specialHigh;
            default:
                Debug.LogError("传入的层级为空！");
                return null;
        }
    }
    /// <summary>
    /// 显示面板（异步加载）
    /// </summary>
    /// <typeparam name="T">预制体名就是面板的类名</typeparam>
    /// <param name="layer">面板要放置的层级</param>
    /// <param name="OnShowPanelFadePre">回调函数在OnShowFadedComplete之前 面板还未显示之前执行 用于在外部初始化</param>
    /// <param name="fadingDuration">面板淡入事件</param>
    public void ShowPanel<T>(E_UILayer layer, Action<T> OnShowPanelFadePre = null, Action<T> OnShowPanelFaded = null, float fadingDuration = 0.2f) where T : MonoBehaviour, IPanel
    {
        string name = typeof(T).Name;
        PanelInfo<T> info;
        if (!_panelDict.ContainsKey(name))
        {
            //用这个PanelInfo信息类来存储到字典中
            info = new PanelInfo<T>();
            info.OnShowPanelFadePre += OnShowPanelFadePre;
            info.OnShowPanelFaded += OnShowPanelFaded;

            _panelDict.Add(name, info);
            #region AB包加载（现在不需要使用）
            //加载面板预制体（面板预制体一般放在AB路径下）
            //ABResMgr.Instance.LoadResAsync<GameObject>("ui", name, (obj) =>
            //{
            //    //如果玩家在加载成功前就把面板关闭了
            //    if (info.isHide)
            //    {
            //        //直接从字典中移除就可以 因为没有实例化所以也不需要从场景中移除对象
            //        panelDict.Remove(name);
            //        return;
            //    }
            //    //参数3：是否保持子物体相对于父对象的位置（false-重置，子坐标归零；true-保持，世界坐标不变 子坐标重新计算为当前世界坐标的位置）
            //    T panel = GameObject.Instantiate(obj, GetBaseLayer(layer), false).GetComponent<T>();
            //    info.panel = panel;
            //    panel.ShowMe();//可以在这里面初始化信息
            //    info.onPanelShowCallback?.Invoke(panel);
            //    info.onPanelShowCallback = null;
            //}, isAsync);
            #endregion
            //加载面板预制体
            ResourcesManager.Instance.LoadAsync<GameObject>(name, (obj) =>
            {
                //如果玩家在加载成功前就把面板关闭了
                if (info.IsLoadPreHide)
                {
                    //直接从字典中移除就可以 因为没有实例化所以也不需要从场景中移除对象
                    _panelDict.Remove(name);
                    return;
                }

                //实例化面板
                //参数3：是否保持子物体相对于父对象的位置（false-重置，子坐标归零；true-保持，世界坐标不变 子坐标重新计算为当前世界坐标的位置）
                T panel = GameObject.Instantiate(obj, GetBaseLayer(layer), false).GetComponent<T>();
                panel.gameObject.name = name;//更新面板名称

                //初始化info信息
                info.Panel = panel;
                info.Widget = panel.GetComponent<Widget>();

                //淡入面板
                FadingPanel(info, fadingDuration);
            });
        }
        else
        {
            //直接从字典中取出面板信息
            info = _panelDict[name] as PanelInfo<T>;

            //还在加载中
            if (info.Panel == null)
            {
                //如果玩家操作很快 刚打开面板然后又关闭了面板 接着再次打开了面板
                //这时面板还未加载完毕 及时更改标识 面板可以正常继续加载完毕
                //这里避免了极端情况下重复的异步加载
                info.IsLoadPreHide = false;
                return;
            }
            //添加回调
            info.OnShowPanelFadePre += OnShowPanelFadePre;
            info.OnShowPanelFaded += OnShowPanelFaded;

            //激活面板
            info.Panel.gameObject.SetActive(true);
            info.Panel.transform.SetParent(GetBaseLayer(layer), false);

            //淡入面板
            FadingPanel(info, fadingDuration);
        }


    }
    private void FadingPanel<T>(PanelInfo<T> info, float fadingDuration) where T : MonoBehaviour, IPanel
    {
        //淡入
        info.Widget.Fade(0f, 0f); //初始化Alpha

        //执行Fade前回调（面板外部的逻辑）
        info.OnShowPanelFadePre?.Invoke(info.Panel);
        info.OnShowPanelFadePre = null;

        //在面板完全显示前执行（面板内部的逻辑）
        info.Panel.OnShowFadePreComplete();

        info.Widget.Fade(1, fadingDuration, () =>
        {
            //更新为已显示
            info.IsDisplaying = true;

            //执行Fade后回调
            //调用回调函数（面板外部的逻辑）
            info.OnShowPanelFaded?.Invoke(info.Panel);
            //清空回调
            info.OnShowPanelFaded = null;
        });
    }
    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="OnHidePanelFaded">面板完全隐藏后（销毁或者失活后）调用 这个回调给与外界的信号就是 隐藏面板的操作已经彻底完成
    /// 同样在OnHideFadedComplete之后执行 用于处理外部逻辑</param>
    /// <param name="fadingDuration">淡出时间</param>
    /// <param name="isDestory"></param>
    public void HidePanel<T>(Action<T> OnHidePanelFadePre = null, Action OnHidePanelFaded = null, float fadingDuration = 0.2f, bool isDestory = false) where T : MonoBehaviour, IPanel
    {
        string name = typeof(T).Name;
        if (_panelDict.ContainsKey(name))
        {
            PanelInfo<T> info = _panelDict[name] as PanelInfo<T>;

            //还未加载完（根本没有显示，无需淡出）
            if (info.Panel == null)
            {
                //此处标记为隐藏 将在实例化时被直接移除
                info.IsLoadPreHide = true; //标记为隐藏
                info.OnShowPanelFaded = null;//清空回调

                //面板为空，不调用OnHideComplete
                //还没加载完直接调用委托
                OnHidePanelFaded?.Invoke();
                return;
            }

            //面板在显示 正常执行逻辑
            if (info.IsDisplaying)
            {
                //面板在场景中成功显示了
                //调用Fade前回调（面板外部逻辑）
                OnHidePanelFadePre?.Invoke(info.Panel);

                info.Widget.Fade(0f, fadingDuration, () =>
                {
                    //在淡出动画完成后 但还未真正隐藏面板 时调用（面板内部逻辑）
                    info.Panel.OnHideFadedComplete();

                    //更新为未显示
                    info.IsDisplaying = false;
                    //销毁
                    if (isDestory)
                    {
                        GameObject.Destroy(info.Panel.gameObject);
                        _panelDict.Remove(name);
                    }
                    //失活
                    else
                        info.Panel.gameObject.SetActive(false);
                    //调用Fade后回调（面板外部逻辑）
                    OnHidePanelFaded?.Invoke();
                });
            }
            //面板未在显示
            else
            {
                //调用Fade后回调
                OnHidePanelFaded?.Invoke();
            }


        }
        else
        {
            //字典中没有该类 直接调用Fade后回调
            OnHidePanelFaded?.Invoke();
        }
    }
    /// <summary>
    /// 获取面板 
    /// 若面板为隐藏 或者 不存在该面板 不会发生任何事
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callback"></param>
    public void GetPanel<T>(Action<T> callback) where T : MonoBehaviour, IPanel
    {
        string name = typeof(T).Name;
        if (_panelDict.ContainsKey(name))
        {
            PanelInfo<T> info = _panelDict[name] as PanelInfo<T>;
            //面板还没加载完成 将回调函数添加到显示完成执行的回调里 显示完成后执行
            if (info.Panel == null)
            {
                info.OnShowPanelFaded += callback;
            }
            else if (!info.IsLoadPreHide)
            {
                callback?.Invoke(info.Panel);
            }
        }
    }
    /// <summary>
    /// 检查面板是否处于显示状态
    /// </summary>
    /// <returns></returns>
    public bool CheckPanelIsShowing<T>() where T : MonoBehaviour, IPanel
    {
        string name = typeof(T).Name;
        if (_panelDict.ContainsKey(name))
        {
            return _panelDict[name].IsDisplaying;
        }
        return false;
    }

    /// <summary>
    /// 添加自定义事件监听器
    /// OnEnable添加 OnDisable移除
    /// 这里是为了给Button等常用触发的控件 添加触发拖拽等特殊操作
    /// 和为了Image等不常添加触发的控件 添加操作使用的
    ///
    /// 这里写成了静态方法 方便外部使用
    /// 因为该方法是为所有控件添加事件监听器的
    /// 跟UI关系不大
    /// </summary>
    /// <param name="control">想添加事件监听的Trigger</param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public static void AddCustomEventLisiener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        //保证EventTrigger组件的唯一性
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = control.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = trigger.triggers.Find(x => x.eventID == type);
        if (entry == null)
        {
            entry = new EventTrigger.Entry();
            entry.eventID = type;
            trigger.triggers.Add(entry);
        }
        entry.callback.AddListener(callback);
    }
    public static void RemoveCustomEventLisiener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger trigger = control.GetComponent<EventTrigger>();
        if (trigger == null) return;

        EventTrigger.Entry entry = trigger.triggers.Find(x => x.eventID == type);
        if (entry != null)
            entry.callback.RemoveListener(callback);
    }
}