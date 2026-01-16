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
            _container.Add(dataField);
            return _container;
        
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