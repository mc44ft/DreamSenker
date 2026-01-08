using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[NodeMenuItem("Entry State")]
public class EntryState : State
{
    public override Color MyColor { get; protected set; } = new Color(144f / 255f, 180f / 255f, 75f / 255f);

    public override void Init(string uniqueID, Vector2 viewPosition)
    {
        base.Init(uniqueID, viewPosition);
        SetTitle("Entry State");
    }
}
