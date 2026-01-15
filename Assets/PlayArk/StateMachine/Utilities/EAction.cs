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
    }
}