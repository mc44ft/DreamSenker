using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
/// <summary>
/// 玩家赤手空拳形态
/// </summary>
public class FormUnarmed : FormStrategy
{
    public override void DoAction(EAction action, string[] parameters)
    {
        //这里在形态内做了保护（已经在ActionState中做了处理，这里可以再做一层安全保护)
        if(!this.enabled) return;

        switch (action)
        {
            case EAction.StopMove:
                _mover.StopMove();
                break;
            case EAction.Move:
                _mover.Move();
                break;
            case EAction.JumpEnterSetup:
                _mover.JumpEnterSetup();
                break;
            case EAction.FallEnterSetup:
                _mover.FallEnterSetup();
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
