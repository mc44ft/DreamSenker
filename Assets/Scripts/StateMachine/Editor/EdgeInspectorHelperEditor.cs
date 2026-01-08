using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(EdgeInspectorHelper))]
public class EdgeInspectorHelperEditor : Editor
{
    private VisualElement _container;
    public override VisualElement CreateInspectorGUI()
    {
        _container = new VisualElement();
        
        SerializedProperty data = serializedObject.FindProperty("Data");
        PropertyField dataField = new PropertyField(data);
        _container.Add(dataField);
        return _container;
        
    
        // ListView listView = new ListView
        // {
        //     headerTitle = "Transition", //设置标题名称（页眉）
        //     bindingPath = "Data", //绑定EdgeInspectorHelper里的Data这个字段
        //     showBorder = true, //在整个ListView控件周围绘制一个细边框
        //     showFoldoutHeader = true, //让列表带一个折叠箭头 在headerTitle的旁边
        //     showBoundCollectionSize = false, //决定了是否在页眉旁边显示列表元素的数量
        //     horizontalScrollingEnabled = true, //当列表内部的内容宽度超过了Inspector窗口的宽度时，允许出现水平滚动条
        //     //动态高度 当列表中的元素过多时 只会生成屏幕里可以显示的几个元素 剩下的动态复用 节省性能
        //     virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
        // };
        
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
