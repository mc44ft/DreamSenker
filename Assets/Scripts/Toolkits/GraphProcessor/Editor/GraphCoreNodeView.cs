using PlayArk.GraphCore.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace PlayArk.GraphCore.Editor
{
    public class GraphCoreNodeView : Node
    {
        public GraphCoreNode CoreNode { get; }

        private VisualElement _borderContainer;

        private List<PortData> dynamicInputs = new List<PortData>();
        private List<PortData> dynamicOutputs = new List<PortData>();

        public GraphCoreNodeView(GraphCoreNode data) : base(GraphCoreEditor.GetPath() + "GraphCoreNode.uxml")
        {
            CoreNode = data;

            viewDataKey = data.UniqueID;

            style.left = data.ViewPosition.x;
            style.top = data.ViewPosition.y;

            _borderContainer = this.Q<VisualElement>("node-border");

            //添加uss类选择器
            _borderContainer.AddToClassList("node");
        }



        public Port AddDunamicPort(PortData data, Direction direction)
        {
            Port port = CreatePort(direction, Port.Capacity.Single);

            //绑定数据层
            port.viewDataKey = data.PortID;
            port.portName = data.ProtName;

            if(direction == Direction.Input)
                inputContainer.Add(port);
            else
                outputContainer.Add(port);

            //RefreshPorts();
            //RefreshExpandedState();
            return port;
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity)
        {
            Port port = Port.Create<Edge>(Orientation.Horizontal, direction, capacity, typeof(bool));
            return port;
        }
    }
}

