using UnityEngine;

public class InputInfo
{
    public enum E_ControlType
    {
        /// <summary>
        /// 键盘
        /// </summary>
        KeyBoard,

        /// <summary>
        /// 鼠标
        /// </summary>
        Mouse,

        /// <summary>
        /// 手柄
        /// </summary>
        Gamepad,
    }

    public enum E_InputType
    {
        /// <summary>
        /// 抬起
        /// </summary>
        Up,

        /// <summary>
        /// 长按
        /// </summary>
        Always,

        /// <summary>
        /// 按下
        /// </summary>
        Down,
    }

    public E_ControlType ControlType;
    public E_InputType InputType;
    public KeyCode KeyCode;
    public int MouseID;

    public InputInfo(E_InputType inputType, KeyCode keycode)
    {
        this.ControlType = E_ControlType.KeyBoard;
        this.InputType = inputType;
        this.KeyCode = keycode;
    }

    public InputInfo(E_InputType inputType, int mouseID)
    {
        this.ControlType = E_ControlType.Mouse;
        this.InputType = inputType;
        this.MouseID = mouseID;
    }
}