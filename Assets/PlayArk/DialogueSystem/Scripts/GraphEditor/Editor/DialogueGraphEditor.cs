
using DialogueSystem.Data;
using PlayArk.GraphCore.Editor;
using System;
using PlayArk.DialogueSystem.Data;
using UnityEditor;
using UnityEditor.Callbacks;
namespace PlayArk.DialogueSystem.Editor
{
    public class DialogueGraphEditor : GraphCoreEditor
    {
        protected override GraphCoreView CreateView()
        {
            return new DialogueGraphView();
        }
        [OnOpenAsset(0)]
        private static bool OnDialogueGraphOpened(int instanceID)
        {
            if(EditorUtility.InstanceIDToObject(instanceID) is DialogueGraph)
            {
                GetWindow<DialogueGraphEditor>(false, "DialogueGraph");
                return true;
            }
            return false;
        }
    }

}
