using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct EmptyEventArgs : IEventArgs
{

}
// int包裹
public struct IntEventArgs : IEventArgs
{
    public int Value;
    public IntEventArgs(int value) { this.Value = value; }
}

// float包裹
public struct FloatEventArgs : IEventArgs
{
    public float Value;
    public FloatEventArgs(float value) { this.Value = value; }
}

// string包裹
public struct StringEventArgs : IEventArgs
{
    public string Value;
    public StringEventArgs(string value) { this.Value = value; }
}

// bool包裹
public struct BoolEventArgs : IEventArgs
{
    public bool Value;
    public BoolEventArgs(bool value) { this.Value = value; }
}
public struct Vector3EventArgs : IEventArgs
{
    public Vector3 Value;
    public Vector3EventArgs(Vector3 value) { this.Value = value; }
}
// GameObject包裹
public struct GameObjectEventArgs : IEventArgs
{
    public GameObject Value;
    public GameObjectEventArgs(GameObject value) { this.Value = value; }
}
// InputMgr轴向输入包裹
public struct InputEventArgs : IEventArgs
{
    public float HorizontalValue;
    public float HorizontalRawValue;
    public float VerticalValue;
    public float VerticalRawValue;
    public float MouseX_Value;
    public float MouseX_RawValue;
    public float MouseY_Value;
    public float MouseY_RawValue;
    public float MouseScrollWheelValue;
}
public struct SceneEventArgs : IEventArgs
{
    public float Progress { get; set; }

    public SceneEventArgs(float progress)
    {
        this.Progress = progress;
    }

}