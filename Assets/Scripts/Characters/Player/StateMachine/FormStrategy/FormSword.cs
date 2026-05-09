using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
using DreamSenker.Managers;

/// <summary>
/// 玩家持剑形态
/// </summary>

namespace DreamSenker.Characters.Player
{
public class FormSword : FormStrategy
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
                _mover.StopMove();
                break;
            case EAction.Move:
                _mover.Move(InputManager.Instance.HorizontalValue);
                break;
            case EAction.JumpEnterSetup:
                _jumper.JumpEnterSetup();
                break;
            case EAction.FallEnterSetup:
                _jumper.FallEnterSetup();
                break;
            case EAction.PlayAnimation:
                _animationPlayer.PlayAnimation(parameters[0]);
                break;
            case EAction.Knockback:
                _health.DoKnockback();
                break;
        }
    }
}
}
