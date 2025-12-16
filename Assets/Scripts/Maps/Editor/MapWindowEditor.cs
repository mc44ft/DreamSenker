using MapSystem.Graph;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
//该窗口类本身也是Unity实例化的一个对象
public class MapWindowEditor : EditorWindow
{
    private MapGraphView m_graphView;

    /// <summary>
    /// 当前选择的图资源对象 如果是static变量 Unity重新编译后会清空
    /// 这里添加可序列化特性 为了让Unity可以保存其值 在重新编译后恢复 保证编辑器窗口不会丢失对象
    /// </summary>
    [SerializeField]
    public MapGraph CurrentSelectedMapGraph; 

    [MenuItem("Window/Node Window/MapNode Window")]
    public static MapWindowEditor OpenWindow()
    {
        //查找Unity编辑器中所有一打开的窗口
        //如果有窗口的类型为 MapNodeWindow 就将其显示在最前面 并赋予其焦点
        //如果没有该窗口 则打开一个新的该类型的编辑器窗口 并将其命名为传入的参数
        return GetWindow<MapWindowEditor>("MapNode Window");//窗口标签的名称
    }
    /// <summary>
    /// 在第一次打开编辑器窗口时调用
    /// 在Unity重新编译时调用
    /// </summary>
    private void CreateGUI()
    {
        //必须要选中一个图资源对象才给new这个GraphView
        //如果直接从菜单打开 未选中任何图对象 将会什么都没有
        BuildGraphView();
    }
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        
        //双击打开
        MapGraph mapGraph = EditorUtility.InstanceIDToObject(instanceID) as MapGraph;
        if (mapGraph != null)
        {
            //创建窗口并打开视图
            MapWindowEditor windowEditor = OpenWindow();
            windowEditor.CurrentSelectedMapGraph = mapGraph;
            windowEditor.BuildGraphView();

            return true;
        }
        return false;
    }
    private void BuildGraphView()
    {
        if (CurrentSelectedMapGraph != null)
        {
            if (m_graphView != null)
            {
                //卸载旧的view
                rootVisualElement.Remove(m_graphView);
            }
            m_graphView = new MapGraphView(CurrentSelectedMapGraph);
            rootVisualElement.Add(m_graphView);
        }
    }
    /// <summary>
    /// 打开窗口时 此函数才生效
    /// 当用户在Hierarchy和Project中切换选择的焦点对象时调用
    /// 在编辑器窗口内部 拖拖拽拽不会调用
    /// </summary>
    private void OnSelectionChange()
    {
        //单击或者双击切换
        MapGraph mapGraph = Selection.activeObject as MapGraph;
        if (mapGraph != null)
        {
            CurrentSelectedMapGraph = mapGraph;
            BuildGraphView();
        }
    }
}
