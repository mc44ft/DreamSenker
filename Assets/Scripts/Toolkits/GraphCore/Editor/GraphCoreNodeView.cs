using PlayArk.GraphCore.Data;
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
        public GraphCoreNode CoreNode { get; private set; }

        //持有主资源的引用
        private GraphCoreGraph _graphCore;
        
        private VisualElement _headerContainer;
        private VisualElement _middleContainer;

        private Button _addInputPortButton;
        private Button _removeInputPortButton;
        private Button _addOutputPortButton;
        private Button _removeOutputPortButton;

        private Dictionary<string, GraphCorePortTemplate> _dynamicPortLookup = new();

        public GraphCoreNodeView() : base(GraphCoreEditor.GetPath() + "GraphCoreNode.uxml")
        {
            
        }
        public virtual void Init(GraphCoreNode data, GraphCoreGraph graphCore)
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
            SetCapabilites();
        }

        private void FindVisualElement()
        {
            _headerContainer = this.Q<VisualElement>("node-header");
            _middleContainer = this.Q<VisualElement>("node-middle");
            _addInputPortButton = this.Q<Button>("button-addInputPort");
            _removeInputPortButton = this.Q<Button>("button-removeInputPort");
            _addOutputPortButton = this.Q<Button>("button-addOutputPort");
            _removeOutputPortButton = this.Q<Button>("button-removeOutputPort");
        }
        protected virtual void SetStyle()
        {
            _headerContainer.AddToClassList("node-header");
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
                    var portView = CreatePortView(port, GetPortCapacity());
                    AddPortView(portView, port.GetDirection());
                }
                foreach (var port in CoreNode.GetOutputPorts())
                {
                    var portView = CreatePortView(port, GetPortCapacity());
                    AddPortView(portView, port.GetDirection());
                }
            }
        }
        /// <summary>
        /// 添加动态端口按钮监听事件
        /// </summary>
        private void AddEventListenenr()
        {
            if (_addInputPortButton != null)
                _addInputPortButton.RegisterCallback<ClickEvent>(OnAddInputPort);
            if (_removeInputPortButton != null)
                _removeInputPortButton.RegisterCallback<ClickEvent>(OnRemoveInputPort);
            if (_addOutputPortButton != null)
                _addOutputPortButton.RegisterCallback<ClickEvent>(OnAddOutputPort);
            if (_removeOutputPortButton != null)
                _removeOutputPortButton.RegisterCallback<ClickEvent>(OnRemoveOutputPort);
        }
        /// <summary>
        /// 用于设置一些特殊功能
        /// 例如：关闭节点的可删除功能
        /// </summary>
        protected virtual void SetCapabilites() { }

        protected virtual Port.Capacity GetPortCapacity() => Port.Capacity.Single;
        private void OnAddInputPort(ClickEvent evt)
        {
            AddPort(E_PortDirection.Input);
        }
        private void OnRemoveInputPort(ClickEvent evt)
        {
            if (inputContainer.childCount > 1)
                RemovePort(E_PortDirection.Input);
        }
        private void OnAddOutputPort(ClickEvent evt)
        {
            AddPort(E_PortDirection.Output);
        }
        private void OnRemoveOutputPort(ClickEvent evt)
        {
            if (outputContainer.childCount > 1)
                RemovePort(E_PortDirection.Output);
        }
        private void AddPort(E_PortDirection portDirection)
        {
            GraphCorePort portData = CoreNode.CreatePortInternal(portDirection);
            EditorUtility.SetDirty(CoreNode);
            var portView = CreatePortView(portData, GetPortCapacity());
            AddPortView(portView, portDirection);
        }
        private void RemovePort(E_PortDirection portDirection)
        {
            CoreNode.DeletePortInternal(portDirection);
            RemovePortView(portDirection);
        }
        private GraphCorePortTemplate CreatePortView(GraphCorePort portData, Port.Capacity capacity)
        {
            GraphCorePortTemplate graphCorePort = new GraphCorePortTemplate();
            graphCorePort.Initialize(portData, capacity);
            return graphCorePort;
        }

        private void AddPortView(GraphCorePortTemplate portView, E_PortDirection portDirection)
        {
            if(portDirection == E_PortDirection.Input)
            {
                inputContainer.Add(portView);
            }
            else
            {
                outputContainer.Add(portView);
            }
        }
        private void RemovePortView(E_PortDirection portDirection)
        {
            if(portDirection == E_PortDirection.Input) 
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
            Undo.RecordObject(CoreNode, "你刚刚移动了一个GraphCoreNode");
            CoreNode.SetPosition(new Vector2(newPos.x, newPos.y));
            EditorUtility.SetDirty(CoreNode);
        }

        public void SetHeaderColor(Color color)
        {
            _headerContainer.style.backgroundColor = color;
        }
    }
}

