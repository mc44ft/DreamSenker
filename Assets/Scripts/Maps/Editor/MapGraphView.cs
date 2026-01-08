using MapSystem.Graph;
using MapSystem.Nodes;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGraphView : GraphCoreView<GraphCoreNodeView<GraphCoreEdgeView>, GraphCoreEdgeView>
{
    protected override bool InterceptionGraph(GraphCoreGraph graphCore)
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
