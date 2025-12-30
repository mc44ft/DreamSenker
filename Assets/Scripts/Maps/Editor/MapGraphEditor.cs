using MapSystem.Graph;
using MapSystem.Nodes;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEditor.Callbacks;

public class MapGraphEditor : GraphCoreEditor
{
    protected override GraphCoreView CreateView()
    {
        return new MapGraphView();
    }
    //优先级高于父类
    [OnOpenAsset(0)]
    private static bool OnOpenMapAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if(obj is MapGraph)
        {
            GetWindow<MapGraphEditor>(false, "MapGraph");
            return true;
        }
        return false;
    }
}
