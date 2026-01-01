using MapSystem.Graph;
using MapSystem.Nodes;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGraphView : GraphCoreView
{
    protected override bool InterCeptionGraph(GraphCoreGraph graphCore)
    {
        if (!(graphCore is MapGraph))
            return true;
        return false;
    }

    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<GraphCoreNode<MapGraphPort>>();
    }
}
