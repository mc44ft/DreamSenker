using PlayArk.GraphCore.Data;
using System;
using System.Collections.Generic;
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
        private Button _removeInputPortButton;
        private Button _addOutputPortButton;
        private Button _removeOutputPortButton;

        private Dictionary<string, GraphCorePortTemplate> _dynamicPortLookup = new();

        public GraphCoreNodeView(GraphCoreNode data, GraphCoreSO graphCore) : base(GraphCoreEditor.GetPath() + "GraphCoreNode.uxml")
        {
            CoreNode = data;
            _graphCore = graphCore;

            viewDataKey = data.GetUniqueID();

            style.left = data.GetViewPosition().x;
            style.top = data.GetViewPosition().y;


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
            _removeInputPortButton = this.Q<Button>("button-removeInputPort");
            _addOutputPortButton = this.Q<Button>("button-addOutputPort");
            _removeOutputPortButton = this.Q<Button>("button-removeOutputPort");
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
                foreach (var port in CoreNode.GetInputPorts())
                {
                    CreatePortView(port, Port.Capacity.Single);
                }
                foreach (var port in CoreNode.GetOutputPorts())
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
            if (_removeInputPortButton != null)
                _removeInputPortButton.clicked += OnRemoveInputPort;
            if (_addOutputPortButton != null)
                _addOutputPortButton.clicked += OnAddOutputPort;
            if (_removeOutputPortButton != null)
                _removeOutputPortButton.clicked += OnRemoveOutputPort;
        }

        

        

        private void OnAddInputPort()
        {
            AddPort(Direction.Input);
        }
        private void OnRemoveInputPort()
        {
            if(inputContainer.childCount > 1)
                RemovePort(Direction.Input);
        }
        private void OnAddOutputPort()
        {
            AddPort(Direction.Output);
        }
        private void OnRemoveOutputPort()
        {
            if(outputContainer.childCount > 1)
                RemovePort(Direction.Output);
        }
        private void AddPort(Direction portDirection)
        {
            GraphCorePort portData = CoreNode.CreatePortInternal(portDirection);
            CreatePortView(portData, Port.Capacity.Single);
        }
        private void RemovePort(Direction portDirection)
        {
            CoreNode.DeletePortInternal(portDirection);
            RemovePortView(portDirection);
        }
        private GraphCorePortTemplate CreatePortView(GraphCorePort portData, Port.Capacity capacity)
        {
            GraphCorePortTemplate graphCorePort = new GraphCorePortTemplate();
            graphCorePort.Initialize(portData);

            if(portData.GetDirection() == Direction.Input)
            {
                inputContainer.Add(graphCorePort);
            }
            else
            {
                outputContainer.Add(graphCorePort);
            }
                
            return graphCorePort;
        }
        private void RemovePortView(Direction portDirection)
        {
            if(portDirection == Direction.Input) 
                inputContainer.RemoveAt(inputContainer.childCount - 1);
            else
                outputContainer.RemoveAt(outputContainer.childCount - 1);
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
                Where(p => p.PortData?.GetUniqueID() == portID).
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

