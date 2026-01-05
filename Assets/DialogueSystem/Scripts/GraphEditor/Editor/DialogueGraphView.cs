using DialogueSystem.Data;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;

public class DialogueGraphView : GraphCoreView<DialogueNodeView>
{
    protected override bool InterceptionGraph(GraphCoreGraph graphCore)
    {
        if (!(graphCore is DialogueGraph))
            return true;
        return false;
    }

    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<DialogueNodeBase>();
    }

    protected override GraphCoreNodeView DrawNode(GraphCoreNode node)
    {
        GraphCoreNodeView nodeView = base.DrawNode(node);
        if(node is DialogueNodeEntry)
            nodeView.SetHeaderColor(new Color(48 / 255f, 90 / 255f, 86 / 255f));
        else if(node is DialogueNodeChoice)
            nodeView.SetHeaderColor(new Color(173 / 255f, 161 / 255f, 86 / 255f));
        return nodeView;
    }
    
}
