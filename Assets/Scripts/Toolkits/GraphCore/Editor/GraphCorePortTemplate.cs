using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphCorePortTemplate : VisualElement
{
    public new class UxmlFactory : UxmlFactory<GraphCorePortTemplate, UxmlTraits> { };

    public Port ComponentPort {  get; private set; }
    private VisualElement _portSlot;
    private TextField _textField;
    private VisualElement _mainContainer;
    public GraphCorePort PortData => ComponentPort?.userData as GraphCorePort;

    public GraphCorePortTemplate()
    {
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Path.Combine(GraphCoreEditor.GetPath(), "GraphCorePort.uxml"));
        visualTree.CloneTree(this);

        _portSlot = this.Q<VisualElement>("portSlot");
        _textField = this.Q<TextField>();
        _mainContainer = this.Q<VisualElement>("mainContainer");
        
    }
    public void Initialize(GraphCorePort portData)
    {
        //创建端口
        ComponentPort = Port.Create<GraphCoreEdgeView>(Orientation.Horizontal, portData.Direction, Port.Capacity.Single, typeof(bool));
        ComponentPort.viewDataKey = portData.UniqueID;
        ComponentPort.userData = portData;
        ComponentPort.portName = " --- ";

        //将端口放入容器
        _portSlot.Add(ComponentPort);

        //绑定输入框逻辑
        _textField.value = portData.PortName;
        _textField.RegisterValueChangedCallback(evt =>
        {
            portData.SetPortName(evt.newValue);
        });

        //更新端口排列方向
        if(portData.Direction == Direction.Input)
        {
            _mainContainer.style.flexDirection = FlexDirection.Row;
        }
        else
        {
            _mainContainer.style.flexDirection = FlexDirection.RowReverse;
        }
    }

}
