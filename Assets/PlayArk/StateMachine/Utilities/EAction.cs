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
        Move,
        JumpEnterSetup,
        FallEnterSetup,
        //--------------- GetHit -----------
        Knockback,
        //--------------- Switch Form -----------
        SwitchFormHandle,//监测切换形态
        //--------------- Attack -----------
        Attack,//触发攻击
    }
}