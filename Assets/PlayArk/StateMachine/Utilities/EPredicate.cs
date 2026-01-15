using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    public enum EPredicate
    {
        KeyCodePressed,
        AnimOver,
        HorizontalNotZero,
        VerticalSpeedNotNegative,
        JumpCountNotZero,
        Grounded, 
        TakeDamage,
        Dead,
    }
}