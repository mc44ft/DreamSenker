using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    public enum EAction
    {
        //--------------- Animation -----------
        PlayAnimation,
        //--------------- Move -----------
        StopMove,
        Move,//FixedUpdate
        JumpEnterSetup,
        FallEnterSetup,
        //--------------- GetHit -----------
        Knockback,
        //--------------- Switch Form -----------
        SwitchFormHandle,//监测切换形态
    }
}