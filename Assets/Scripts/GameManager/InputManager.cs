using UnityEngine;

public class InputManager : SingletonAutoMono<InputManager>
{
    private bool _playerInputAction = true;
    private bool _uiInputAction = true;

    //参数/属性轮询的方案
    //-------- 持续性输入 参数驱动 -------------
    public float HorizontalValue { get; set; }
    public float VerticalValue { get; set; }
    //-------- 瞬间输入 参数驱动 -------------
    public bool JumpButtonDown { get; private set; }
    public bool JumpButtonHold { get; private set; }
    public bool JumpButtonUp { get; private set; }
    public bool AttackButtonDown { get; private set; }
    public bool AttackButtonUp { get; private set; }
    public bool SwitchFormButtonDown { get; private set; }
    public bool PickupButtonDown { get; private set; }
    public bool UpButtonDown { get; private set; }
    
    //-------- UI输入 事件驱动 --------------

    //-------- Private Parameter -------------
    private bool _upButtonDownPre;
    private void Update()
    {
        //---------------- 玩家操作输入 -----------------
        if (_playerInputAction)
        {
            PlayerInput();
            
        }
        //---------------- UI按键 -----------------
        if (_uiInputAction)
        {
            UiInput();
        }

        //---------------- 全局按键 -----------------
        //设置面板可被随时呼唤 不论是否暂停玩家输入
        if (Input.GetButtonDown("Settings"))
        {
            //通知外部设置键按下
            EventCenter.Instance.EventTrigger(E_EventType.InputUI_SettingsPanel, this, new EmptyEventArgs());
        }

    }
    private void PlayerInput()
    {
        HorizontalValue = Input.GetAxisRaw("Horizontal");
        VerticalValue = Input.GetAxisRaw("Vertical");

        //等下一帧没有按下按钮 这里自动变为false了 外部只需要监测这个变量何时为true就行
        JumpButtonDown = Input.GetButtonDown("Jump");
        JumpButtonHold = Input.GetButton("Jump");
        JumpButtonUp = Input.GetButtonUp("Jump");
        AttackButtonDown = Input.GetButtonDown("Attack");
        AttackButtonUp = Input.GetButtonUp("Attack");
        SwitchFormButtonDown = Input.GetButtonDown("SwitchForm");
        PickupButtonDown = Input.GetButtonDown("Pickup");

        if(VerticalValue >= 0.5f)//向上推摇杆
        {
            if (!_upButtonDownPre)//上一帧是false
                UpButtonDown = true;//本次变true
            else
                UpButtonDown = false;//否则本次是false
            _upButtonDownPre = true;//记录上一帧状态
        }
        else
        {
            UpButtonDown = false;
            _upButtonDownPre = false;//记录上一帧状态
        }
    }
    private void UiInput()
    {
        if (Input.GetButtonDown("Package"))
        {
            //通知外部背包键按下
            EventCenter.Instance.EventTrigger(E_EventType.InputUI_PackagePanel, this, new EmptyEventArgs());
        }
    }
    public void SetPlayerInputAction(bool action)
    {
        _playerInputAction = action;

        if(!_playerInputAction)
        {
            HorizontalValue = 0f;
            //重置按键状态
            UpButtonDown = false;
        }
    }
    public void SetUiInputAction(bool action)
    {
        _uiInputAction = action;
    }
    public enum E_InputDeviceType
    {
        KeyboardMouse,
        Gamepad,
    }
}
