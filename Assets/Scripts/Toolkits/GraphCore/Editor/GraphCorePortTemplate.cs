using PlayArk.GraphCore.Data;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayArk.GraphCore.Editor
{
    public class GraphCorePortTemplate : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<GraphCorePortTemplate, UxmlTraits> { };

        public Port ComponentPort { get; private set; }
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
        public void Initialize<TEdgeView>(GraphCorePort portData, Port.Capacity capacity)
            where TEdgeView : GraphCoreEdgeView, new()
        {
            //创建端口
            //在这里指定了连线的类型
            ComponentPort = Port.Create<TEdgeView>(
                Orientation.Horizontal,
                portData.GetDirection() == E_PortDirection.Input ? Direction.Input : Direction.Output, 
                capacity,
                typeof(bool));
            ComponentPort.viewDataKey = portData.GetUniqueID();
            ComponentPort.userData = portData;
            ComponentPort.portName = " --- ";

            //将端口放入容器
            _portSlot.Add(ComponentPort);

            //绑定输入框逻辑
            _textField.value = portData.GetPortName();
            _textField.RegisterValueChangedCallback(evt =>
            {
                portData.SetPortName(evt.newValue);
            });

            //更新端口排列方向
            if (portData.GetDirection() == E_PortDirection.Input)
            {
                _mainContainer.style.flexDirection = FlexDirection.Row;
            }
            else
            {
                _mainContainer.style.flexDirection = FlexDirection.RowReverse;
            }
        }

    }
}