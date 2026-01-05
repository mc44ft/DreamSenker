using System.Collections;
using System.Collections.Generic;
using MapSystem.Graph;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class StateMachineEditor : GraphCoreEditor
{
    protected override GraphCoreView CreateView()
    {
        return new StateMachineView();
    }
    [OnOpenAsset(0)]
    private static bool OnOpenMapAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if(obj is StateMachine)
        {
            GetWindow<StateMachineEditor>(false, "State Machine Editor", true);
            return true;
        }
        return false;
    }
}
