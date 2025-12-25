using PlayArk.GraphCore.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace PlayArk.GraphCore.Editor
{
    public class GraphCoreNodeView : Node
    {
        public GraphCoreNode CoreNode { get; }

        //持有主资源的引用
        private GraphCoreSO _graphCore;

        private VisualElement _borderContainer;
        private VisualElement _middleContainer;
        private Button _addInputPortButton;
        private Button _addOutputPortButton;

        //private List<Port> _dynamicInputs = new List<Port>();
        //private List<Port> _dynamicOutputs = new List<Port>();

        private Dictionary<string, GraphCorePortTemplate> _dynamicPortLookup = new();

        public GraphCoreNodeView(GraphCoreNode data, GraphCoreSO graphCore) : base(GraphCoreEditor.GetPath() + "GraphCoreNode.uxml")
        {
            CoreNode = data;
            _graphCore = graphCore;

            viewDataKey = data.UniqueID;

            style.left = data.ViewPosition.x;
            style.top = data.ViewPosition.y;


            FindVisualElement();
            SetStyle();//添加uss类选择器
            SetTitle();
            DrawPorts();//绘制所有端口
            AddEventListenenr();
        }
        private void FindVisualElement()
        {
            _borderContainer = this.Q<VisualElement>("node-header");
            _middleContainer = this.Q<VisualElement>("node-middle");
            _addInputPortButton = this.Q<Button>("button-addInputPort");
            _addOutputPortButton = this.Q<Button>("button-addOutputPort");
        }
        private void SetStyle()
        {
            _borderContainer.AddToClassList("node-header");
            _middleContainer.AddToClassList("node-middle");
            inputContainer.AddToClassList("input");
            outputContainer.AddToClassList("output");
        }
        private void SetTitle()
        {
            //label就是一个文本框
            Label titleLabel = this.Q<Label>("title-label");
            //这里是给label绑定了一个叫"_title"的变量 当对应的这个对象里的这个变量修改的时候 label会同步修改为这个变量的值
            titleLabel.bindingPath = "_title";
            //绑定序列化对象
            titleLabel.Bind(new SerializedObject(CoreNode));
        }
        private void DrawPorts()
        {
            if (CoreNode != null)
            {
                foreach (var port in CoreNode.InputPorts)
                {
                    CreatePortView(port, Port.Capacity.Single);
                }
                foreach (var port in CoreNode.OutputPorts)
                {
                    CreatePortView(port, Port.Capacity.Single);
                }
            }
        }
        /// <summary>
        /// 添加动态端口按钮监听事件
        /// </summary>
        private void AddEventListenenr()
        {
            if (_addInputPortButton != null)
                _addInputPortButton.clicked += OnAddInputPort;
            if (_addOutputPortButton != null)
                _addOutputPortButton.clicked += OnAddOutputPort;
        }

        private void OnAddInputPort()
        {
            AddPort(Direction.Input);
        }
        private void OnAddOutputPort()
        {
            AddPort(Direction.Output);
        }
        
        private void AddPort(Direction portDirection)
        {
            GraphCorePort portData = CoreNode.CreatePort(portDirection);
            CreatePortView(portData, Port.Capacity.Single);
        }
        private GraphCorePortTemplate CreatePortView(GraphCorePort portData, Port.Capacity capacity)
        {
            GraphCorePortTemplate graphCorePort = new GraphCorePortTemplate();
            graphCorePort.Initialize(portData);

            if(portData.Direction == Direction.Input)
            {
                inputContainer.Add(graphCorePort);
            }
            else
            {
                outputContainer.Add(graphCorePort);
            }
                
            return graphCorePort;
        }
        public GraphCoreEdgeView ConnectTo(string rootPortID, GraphCoreNodeView trueNodeView, string truePortID)
        {
            Port rootPort = GetPortByID(rootPortID);
            Port truePort = trueNodeView.GetPortByID(truePortID);
            return rootPort.ConnectTo<GraphCoreEdgeView>(truePort);
        }
        public Port GetPortByID(string portID)
        {
            return this.Query<GraphCorePortTemplate>().
                Where(p => p.PortData?.UniqueID == portID).
                First().ComponentPort;
        }
        public override void OnSelected()
        {
            base.OnSelected();
            //当选中该表现层节点时 定位到其对应的数据层节点
            //将Unity编辑器当前选中的全局对象更新为state 即出现在Inspector面板上
            //这里传入了两个参数
            //1.参数一（主角）：这是真正想要选中的对象 也就是当前选中表现层对象的真正内在
            //会交给Inspector窗口绘制
            //2.参数二（背景）：这个是选中对象的背景 告诉Unity，这个主角（参数一）是在什么环境下被选中的
            Selection.SetActiveObjectWithContext(CoreNode, _graphCore);
        }
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            CoreNode.SetPosition(new Vector2(newPos.x, newPos.y));
        }
        
    }
}

