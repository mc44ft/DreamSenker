using DialogueSystem.Data;
using PlayArk.DialogueSystem.Data;
using PlayArk.DialogueSystem.Data.Nodes;
using PlayArk.GraphCore;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;

namespace PlayArk.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphCoreView<DialogueNodeView, GraphCoreEdgeView>
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
    }
}