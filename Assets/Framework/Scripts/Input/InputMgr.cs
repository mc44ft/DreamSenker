using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputMgr : BaseManager<InputMgr>
{
    private Dictionary<E_EventType, InputInfo> _inputDict = new Dictionary<E_EventType, InputInfo>();

    /// <summary>
    /// 整体监测开关
    /// </summary>
    private bool _isOpen = false;

    /// <summary>
    /// 轴监测开关
    /// </summary>
    private bool _isAxisOpen = true;

    /// <summary>
    /// 获取任意键按键信息的回调函数
    /// </summary>
    private UnityAction<InputInfo> _anyKeyCallback = null;

    private bool _isAnyKeyCheck = false;

    private InputInfo _info;

    private InputEventArgs _inputEventArgs;

    private InputMgr()
    {
        MonoManager.Instance.AddUpdateListener(Update);
        _inputEventArgs = new InputEventArgs();
    }

    public void OpenOrClose(bool isOpen)
    {
        this._isOpen = isOpen;
    }

    public void OpenOrCloseAxis(bool isAxisOpen)
    {
        this._isAxisOpen = isAxisOpen;
    }
    /// <summary>
    /// 获取按键信息（用于改键位）
    /// </summary>
    /// <param name="callback"></param>
    public void GetInputInfo(UnityAction<InputInfo> callback)
    {
        _anyKeyCallback = callback;
        //利用协程来延迟一帧获取按键信息
        MonoManager.Instance.StartCoroutine(GetInputInfoCoroutine());
        IEnumerator GetInputInfoCoroutine()
        {
            yield return null;
            _isAnyKeyCheck = true;
        }
    }
    private void Update()
    {
        #region 改键监测
        //没有开启按键监测的情况下 也可以进行改键 也可以获取到按键信息
        if (_isAnyKeyCheck && Input.anyKeyDown)
        {
            InputInfo changeInfo = null;
            //遍历监听所有按键的按下
            //键盘输入
            Array keyCodes = Enum.GetValues(typeof(KeyCode));
            foreach (KeyCode keyCode in keyCodes)
            {
                if (Input.GetKeyDown(keyCode))
                {
                    changeInfo = new InputInfo(InputInfo.E_InputType.Down, keyCode);
                    break;
                }
            }
            //鼠标输入
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    changeInfo = new InputInfo(InputInfo.E_InputType.Down, i);
                    break;
                }
            }
            _anyKeyCallback.Invoke(changeInfo);
            _anyKeyCallback = null;
            _isAnyKeyCheck = false;
        }
        #endregion
        if (!_isOpen)
            return;
        foreach (var eventType in _inputDict.Keys)
        {
            _info = _inputDict[eventType];
            switch (_info.ControlType)
            {
                case InputInfo.E_ControlType.KeyBoard:
                    switch (_info.InputType)
                    {
                        case InputInfo.E_InputType.Up:
                            if (Input.GetKeyUp(_info.KeyCode))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;

                        case InputInfo.E_InputType.Always:
                            if (Input.GetKey(_info.KeyCode))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;

                        case InputInfo.E_InputType.Down:
                            if (Input.GetKeyDown(_info.KeyCode))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;
                    }
                    break;

                case InputInfo.E_ControlType.Mouse:
                    switch (_info.InputType)
                    {
                        case InputInfo.E_InputType.Up:
                            if (Input.GetMouseButtonUp(_info.MouseID))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;

                        case InputInfo.E_InputType.Always:
                            if (Input.GetMouseButton(_info.MouseID))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;

                        case InputInfo.E_InputType.Down:
                            if (Input.GetMouseButtonDown(_info.MouseID))
                                EventCenter.Instance.EventTrigger(eventType, this, new EmptyEventArgs());
                            break;
                    }
                    break;

                case InputInfo.E_ControlType.Gamepad:
                    // 手柄输入暂时不处理
                    break;

                default:
                    break;
            }
        }
        if (_isAxisOpen)
        {
            OnAxis();
        }
    }

    /// <summary>
    /// 初始化或更改键盘键位
    /// </summary>
    /// <param name="eventType">绑定的行为事件</param>
    /// <param name="inputType">输入操作类型</param>
    /// <param name="keyCode">键位</param>
    public void ChangeKeyboardInfo(E_EventType eventType, InputInfo.E_InputType inputType, KeyCode keyCode)
    {
        _inputDict[eventType] = new InputInfo(inputType, keyCode);
    }

    /// <summary>
    /// 初始化或更改鼠标键位
    /// </summary>
    /// <param name="eventType">绑定的行为事件</param>
    /// <param name="inputType">输入操作类型</param>
    /// <param name="mouseID">鼠标键位ID</param>
    public void ChangeMouseInfo(E_EventType eventType, InputInfo.E_InputType inputType, int mouseID)
    {
        _inputDict[eventType] = new InputInfo(inputType, mouseID);
    }

    public void RemoveInputInfo(E_EventType eventType)
    {
        if (_inputDict.ContainsKey(eventType))
            _inputDict.Remove(eventType);
    }

    private void OnAxis()
    {
        _inputEventArgs.HorizontalValue = Input.GetAxis("Horizontal");
        _inputEventArgs.HorizontalRawValue = Input.GetAxisRaw("Horizontal");
        _inputEventArgs.VerticalValue = Input.GetAxis("Vertical");
        _inputEventArgs.VerticalRawValue = Input.GetAxisRaw("Vertical");
        _inputEventArgs.MouseX_Value = Input.GetAxis("Mouse X");
        _inputEventArgs.MouseX_RawValue = Input.GetAxisRaw("Mouse X");
        _inputEventArgs.MouseY_Value = Input.GetAxis("Mouse Y");
        _inputEventArgs.MouseY_RawValue = Input.GetAxisRaw("Mouse Y");
        _inputEventArgs.MouseScrollWheelValue = Input.GetAxis("Mouse ScrollWheel");

        //分发总事件
        EventCenter.Instance.EventTrigger(E_EventType.Input_Axis, this, _inputEventArgs);
    }
}
