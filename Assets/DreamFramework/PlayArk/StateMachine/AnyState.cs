using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.StateMachine
{
    public class AnyState : State
    {
        public override Color GetColor()
            => new Color(134 / 255f, 166 / 255f, 151 / 255f);

        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            base.Init(uniqueID, viewPosition);
            SetTitle("Any State");
        }

    
    }
}