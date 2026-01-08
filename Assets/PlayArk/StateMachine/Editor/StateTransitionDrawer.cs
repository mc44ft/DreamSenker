using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayArk.StateMachine.Editor
{
    [CustomPropertyDrawer(typeof(StateTransitionEdge))]
    public class StateTransitionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new VisualElement();
            if (property.serializedObject.targetObject is EdgeInspectorHelper edge)
            {
                DrawTransitionLabel(property, container, edge);
                DrawConditionProperty(property, container);
            }

            return container;
        }
        /// <summary>
        /// 绘制转换标签
        /// </summary>
        private void DrawTransitionLabel(SerializedProperty property, VisualElement container, EdgeInspectorHelper edge)
        {
            global::PlayArk.StateMachine.StateMachine stateMachine = edge.StateMachine;
            if (stateMachine != null)
            {
                SerializedProperty rootStateID = property.FindPropertyRelative("_rootNodeID");
                SerializedProperty trueStateID = property.FindPropertyRelative("_connectionNodeID");

                State rootState = stateMachine.GetNodeByID(rootStateID.stringValue);
                State trueState = stateMachine.GetNodeByID(trueStateID.stringValue);
            
                Label transitionLabel = new Label($"{rootState.GetTitle()} → {trueState.GetTitle()}");
                container.Add(transitionLabel);
            }
        }

        private void DrawConditionProperty(SerializedProperty property, VisualElement container)
        {
            SerializedProperty condition = property.FindPropertyRelative("_condition");
            PropertyField conditionField = new(condition);
            container.Add(conditionField);
        }
    }
}