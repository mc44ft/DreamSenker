using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayArk.StateMachine.Editor
{
    [CustomEditor(typeof(EdgeInspectorHelper))]
    public class EdgeInspectorHelperEditor : UnityEditor.Editor
    {
        private VisualElement _container;
        public override VisualElement CreateInspectorGUI()
        {
            _container = new VisualElement();

            SerializedProperty data = serializedObject.FindProperty("Data");
            PropertyField dataField = new PropertyField(data);

            // 注册值变更回调，标记主资源为脏以确保持久化
            dataField.TrackPropertyValue(data, OnDataChanged);

            _container.Add(dataField);
            return _container;

        }

        /// <summary>
        /// Condition数据变更时标记StateMachine为脏，支持持久化和撤销
        /// </summary>
        private void OnDataChanged(SerializedProperty property)
        {
            if (target is EdgeInspectorHelper edge && edge.StateMachine != null)
            {
                Undo.RecordObject(edge.StateMachine, "修改连线Condition");
                EditorUtility.SetDirty(edge.StateMachine);
            }
        }

        private VisualElement MakeTransitionItem()
        {
            VisualElement itemContainer = new VisualElement()
            {
                style =
                {
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 5,
                    paddingBottom = 5,
                }
            };
            return itemContainer;
        }

        private void BindTransitionItem(VisualElement element, int index)
        {

        }
    }
}