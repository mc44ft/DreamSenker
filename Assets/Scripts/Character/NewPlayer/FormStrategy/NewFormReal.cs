using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

public class NewFormReal : NewFormStrategy
{
    
    public override void DoAction(EAction action, string[] parameters)
    {
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
