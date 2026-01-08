using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 实现此接口的状态可以通过EAction来调度不同的策略
/// </summary>
public interface IAction
{
    void DoAction(EAction action, string[] parameters);
}
