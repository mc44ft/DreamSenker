using MapSystem.Nodes;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGraphView : GraphCoreView
{
    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<GraphCoreNode<MapGraphPort>>();
    }
}
