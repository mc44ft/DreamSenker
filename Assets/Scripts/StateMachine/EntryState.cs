using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[NodeMenuItem("Entry State")]
public class EntryState : State
{
    public override void Init(string uniqueID, Vector2 viewPosition)
    {
        base.Init(uniqueID, viewPosition);
        SetTitle("Entry State");
    }
}
