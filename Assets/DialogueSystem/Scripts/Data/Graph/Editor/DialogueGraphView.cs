using DialogueSystem.Data;
using PlayArk.GraphCore.Editor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueGraphView : GraphCoreView
{
    protected override void AppendMenuAction(ContextualMenuPopulateEvent evt, Vector2 mousePosition)
    {
        Debug.Log("执行");
        evt.menu.AppendAction("Create DialogueNodeNormal", a => CreateNode(typeof(DialogueNodeNormal), mousePosition));
    }
}
