using System.Collections;
using System.Collections.Generic;
using PlayArk.GraphCore.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class DialogueNodeView : GraphCoreNodeView<GraphCoreEdgeView>
{
    protected override void SetCapabilites()
    {
        base.SetCapabilites();
        if (CoreNode is DialogueNodeEntry)
        {
            capabilities -= Capabilities.Deletable;
        }
    }
}
