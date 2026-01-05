using DialogueSystem.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueNodeEntry : DialogueNodeBase
{
    public override void Init(string uniqueID, Vector2 viewPosition)
    {
        base.Init(uniqueID, viewPosition);
        SetTitle("Entry");
    }

    protected override void OnExecute()
    {
        OnFinished();
    }

    protected override void Finished()
    {
        
    }
}
