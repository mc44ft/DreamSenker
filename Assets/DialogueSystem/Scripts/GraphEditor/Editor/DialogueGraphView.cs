using DialogueSystem.Data;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;

public class DialogueGraphView : GraphCoreView
{
    protected override bool InterCeptionGraph(GraphCoreGraph graphCore)
    {
        if (!(graphCore is DialogueGraph))
            return true;
        return false;
    }

    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<DialogueNodeBase>();
    }
}
