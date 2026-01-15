using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

public class NewFormMirror : NewFormStrategy
{
    [SerializeField] private GameObject _selectedEffect;
    private void OnDisable()
    {
        //关闭Mirror选中特效
        Deselect();
    }
    private void Deselect()
    {
        _selectedEffect.SetActive(false);
    }
    public override void DoAction(EAction action, string[] parameters)
    {
        if(!this.enabled) return;
        switch (action)
        {
            case EAction.StopMove:
                // StopMove();
                break;
            case EAction.Move:
                // Move();
                break;
            case EAction.JumpEnterSetup:
                // JumpEnterSetup();
                break;
            case EAction.FallEnterSetup:
                // FallEnterSetup();
                break;
        }
    }
}
