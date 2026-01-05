using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[NodeMenuItem("Any State")]
public class AnyState : State
{
    public override void Init(string uniqueID, Vector2 viewPosition)
    {
        base.Init(uniqueID, viewPosition);
        SetTitle("Any State");
    }
}
