using DialogueSystem.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueNodeEntry : DialogueNodeBase
{
    protected override void OnExecute()
    {
        Finished();
    }

    protected override void OnFinished()
    {
        
    }
}
