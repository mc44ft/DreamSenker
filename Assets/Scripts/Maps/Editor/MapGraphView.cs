using MapSystem.Nodes;
using PlayArk.GraphCore.Editor;
using UnityEngine;
using UnityEngine.UIElements;

public class MapGraphView : GraphCoreView
{
    protected override void AppendMenuAction(ContextualMenuPopulateEvent evt, Vector2 mousePosition)
    {
        evt.menu.AppendAction("Create MapNode", a => CreateNode(typeof(MapNode), mousePosition));
    }
}
