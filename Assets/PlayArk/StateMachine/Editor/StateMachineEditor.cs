using System;
using System.Collections;
using System.Collections.Generic;
using MapSystem.Graph;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace PlayArk.StateMachine.Editor
{
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
            if(obj is global::PlayArk.StateMachine.StateMachine)
            {
                GetWindow<StateMachineEditor>(false, "State Machine Editor", true);
                return true;
            }
            return false;
        }

        protected override void OnSelectionChange()
        {
            StateMachine stateMachine = Selection.activeObject as StateMachine;
            //如果是点击了Hierarchy窗口中的GameObject游戏对象
            if (Selection.activeGameObject)
            {
                StateMachineController controller = Selection.activeGameObject.GetComponent<StateMachineController>();
                if (controller != null)
                {
                    stateMachine = controller.StateMachine;
                }
            }
            if(stateMachine != null)
            {
                //刷新画布
                _view.Refresh(stateMachine);
            }
            
        }

        /// <summary>
        /// 每秒钟调用十次
        /// </summary>
        private void OnInspectorUpdate()
        {
            if (_view is StateMachineView view)
            {
                view.UpdateStates();
            }
            //刷新UI
            Repaint();
        }
    }
}