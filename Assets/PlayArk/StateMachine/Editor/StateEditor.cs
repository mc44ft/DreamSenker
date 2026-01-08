using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace PlayArk.StateMachine.Editor
{
    /// <summary>
    /// 当点击画布上的某个状态节点时 Inspector窗口该显示什么内容 由这个脚本决定
    /// editorForChildClasses参数为true时 State的子类也继承该样式
    /// </summary>
    [CustomEditor(typeof(State), true)]
    public class StateEditor : UnityEditor.Editor
    {
        private VisualElement _container;
        public override VisualElement CreateInspectorGUI()
        {
            _container = new VisualElement();
            if (serializedObject.targetObject is ActionState)
            {
                DrawProperty("_title");
                DrawProperty("_onEnterActions");
                DrawProperty("_onLogicUpdateActions");
                DrawProperty("_onPhysicsUpdateActions");
                DrawProperty("_onExitActions");
            }
            return _container;
        }

        private void DrawProperty(string propertyName)
        {
            //在序列化对象中通过字段名找到对应的Unity序列化属性（利用反射）
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            //这里根据字段类型智能创建UI控件
            //如果是文本就创建一个输入框
            //如果是bool 就是一个勾选框
            PropertyField field =  new PropertyField(property);
        
            //这里将UI控件和内存数据相绑定
            //当代码修改了资源的值时 UI可以同步改变
            //UI修改了值时 资源的值也能同步改变
            //Bind之后才能自动的支持Undo/Redo操作
            field.Bind(serializedObject);
        
            _container.Add(field);
            //添加一段空白间距
            _container.Add(MakeEmptyLine());
        }

        private VisualElement MakeEmptyLine()
        {
            VisualElement space = new()
            {
                //添加大小为10的下外边距
                style = { marginBottom = 10 }
            };
            return space;
        }
    }
}