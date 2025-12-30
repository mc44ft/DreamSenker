using DialogueSystem.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;

public class DialogueGraphView : GraphCoreView
{
    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<DialogueNodeBase>();
    }
}
