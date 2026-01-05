using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[NodeMenuItem("Action State")]
public class ActionState : State
{
    public override void Init(string uniqueID, Vector2 viewPosition)
    {
        base.Init(uniqueID, viewPosition);
        SetTitle("Action State");
    }
}
