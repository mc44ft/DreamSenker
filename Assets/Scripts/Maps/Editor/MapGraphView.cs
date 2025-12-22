using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MapSystem.Nodes;
using MapSystem.Graph;
public class MapGraphView : GraphView
{
    private MapGraph _mapGraph;
    private Dictionary<MapNode, MapNodeView> _mapNodeViewDict = new Dictionary<MapNode, MapNodeView>();
    private MapGraphView()
    {
        //这句代码让 _graphView 撑满整个窗口
        style.flexGrow = 1;

        GridBackground grid = new GridBackground();
        //添加网格背景元素 
        Insert(0, grid);//将其插入第0层（最底层）


        //缩放
        ContentZoomer contentZoomer = new ContentZoomer(); 
        //设置缩放的最大最小值
        contentZoomer.minScale = 0.2f;
        contentZoomer.maxScale = 2.0f;

        //添加操纵器
        this.AddManipulator(contentZoomer);//缩放
        this.AddManipulator(new ContentDragger());//拖动画布
        this.AddManipulator(new SelectionDragger());//拖动节点
        this.AddManipulator(new RectangleSelector());//框选

        //设置uss样式
        styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Scripts/Maps/Editor/MapGraphViewUSS.uss"));

        //当图标发生任何变化时执行的事件参数
        graphViewChanged += OnGraphViewChanged;

        //注册销毁该View时的事件 该事件中心自动管理订阅的事件 无需手动取消订阅
        RegisterCallback<DetachFromPanelEvent>(OnDetach);
    }

    

    public MapGraphView(MapGraph mapGraph) : this()
    {
        _mapGraph = mapGraph;

        viewDataKey = mapGraph.guid;

        //绘制所有节点
        foreach (var node in mapGraph.Nodes)
        {
            _mapNodeViewDict[node] = DrawNodeView(node);
        }

        //绘制所有端口连线
        foreach(var node in mapGraph.Nodes)
        {
            //只检查输出端口
            MapNode topOutputConnectionNode = node.Connections.TopOutputConnectionNode;
            if (topOutputConnectionNode != null)//该端口连接有另一个节点
            {
                //该节点的TopOutput端口上连接的Node为topOutputConnectionNode
                //因为限制了TopOutput端口只能和BottomInput端口相连接
                //只需要将另外一个Node的TopInput端口与其连接即可
                DrawConnection(node, E_ConnectionType.TopOutput, topOutputConnectionNode, E_ConnectionType.BottomInput);
            }
            MapNode leftOutputConnectionNode = node.Connections.LeftOutputConnectionNode;
            if(leftOutputConnectionNode != null)
            {
                DrawConnection(node, E_ConnectionType.LeftOutput, leftOutputConnectionNode, E_ConnectionType.RightInput);
            }
            MapNode RightOutputConnectionNode = node.Connections.RightOutputConnectionNode;
            if (RightOutputConnectionNode != null)
            {
                DrawConnection(node, E_ConnectionType.RightOutput, RightOutputConnectionNode, E_ConnectionType.LeftInput);
            }
            MapNode BottomOutputConnectionNode = node.Connections.BottomOutputConnectionNode;
            if (BottomOutputConnectionNode != null)
            {
                DrawConnection(node, E_ConnectionType.BottomOutput, BottomOutputConnectionNode, E_ConnectionType.TopInput);
            }
        }
    }
    private MapNodeView DrawNodeView(MapNode node)
    {
        MapNodeView nodeView = new MapNodeView(node);
        AddElement(nodeView);
        return nodeView;
    }
    private void DrawConnection(MapNode outputNode, E_ConnectionType outputType, MapNode inputNode, E_ConnectionType inputType)
    {
        Port outputPort = _mapNodeViewDict[outputNode].GetPortFromConnectionType(outputType);
        Port inputPort = _mapNodeViewDict[inputNode].GetPortFromConnectionType(inputType);
        Edge edge = outputPort.ConnectTo(inputPort);
        AddElement(edge);
    }
    /// <summary>
    /// 当图标发生任何变化时 此回调函数执行
    /// 用于处理Model方的数据处理
    /// </summary>
    /// <param name="graphViewChange"></param>
    /// <returns></returns>
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if(graphViewChange.elementsToRemove != null)
        {
            //遍历所有被删除的元素
            foreach(var element in graphViewChange.elementsToRemove)
            {
                if(element is MapNodeView nodeView)//删除元素为节点
                {
                    //删除Model端的节点信息
                    _mapGraph.DeleteNode(nodeView.MapNode);
                }
                else if(element is Edge edge)//删除元素为连线
                {
                    MapNodeView inputNodeView = edge.input.node as MapNodeView;
                    MapNodeView outputNodeView = edge.output.node as MapNodeView;
                    //获取连线的端口类型
                    E_ConnectionType inputConnectionType = inputNodeView.GetConnectionTypeFromPort(edge.input);
                    E_ConnectionType outputConnectionType = outputNodeView.GetConnectionTypeFromPort(edge.output);

                    //删除Model端的连线信息
                    _mapGraph.DeleteConnection(outputNodeView.MapNode, outputConnectionType, inputNodeView.MapNode, inputConnectionType);
                }
            }
        }
        if(graphViewChange.edgesToCreate != null)
        {
            //遍历所有新创建的连线
            foreach(var edge in graphViewChange.edgesToCreate)
            {
                MapNodeView inputNodeView = edge.input.node as MapNodeView;
                MapNodeView outputNodeView = edge.output.node as MapNodeView;
                //获取连线的端口类型
                E_ConnectionType inputConnectionType = inputNodeView.GetConnectionTypeFromPort(edge.input);
                E_ConnectionType outputConnectionType = outputNodeView.GetConnectionTypeFromPort(edge.output);

                _mapGraph.SaveConnection(outputNodeView.MapNode, outputConnectionType, inputNodeView.MapNode, inputConnectionType);
            }
        }
        return graphViewChange;
    }
    private void OnDetach(DetachFromPanelEvent evt)
    {
        //取消订阅事件 防止内存泄露
        graphViewChanged -= OnGraphViewChanged;
    }
    /// <summary>
    /// 点击右键显示菜单前调用
    /// </summary>
    /// <param name="evt">这是右键点击的事件参数</param>
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        //TypeCache是一个编辑器专用的类
        //在编译代码时 Unity会将所有类、方法、特性等 全部登记在一个索引里
        //TypeCache就是这个索引的名字
        //凭借这个索引可以快速查找对应的对象，性能极高

        //得到Node的所有子类
        //TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom<Node>();
        //foreach(var type in types)
        //{
        //    evt.menu.AppendAction($"{type.Name}", (action) => CreateNode(type));
        //}
        //显示默认选项
        base.BuildContextualMenu(evt);
        //新增选项
        //localMousePosition是屏幕的坐标
        //contentViewContainer是画布容器
        //ChangeCoordinatesTo是将屏幕坐标转换为画布上实际的坐标
        evt.menu.AppendAction("Create MapNode", (action) => CreateNode(
            this.ChangeCoordinatesTo(contentViewContainer, action.eventInfo.localMousePosition)));
    }
    public void CreateNode(Vector2 mousePosition)
    {
        //创建MapNode资源
        MapNode node = _mapGraph.CreateMapNode();
        //创建MapNodeView
        MapNodeView nodeView = new MapNodeView(node);

        //将其创建在鼠标所在位置
        nodeView.SetPosition(new Rect(mousePosition, new Vector2(300, 300)));

        //添加到该View中
        AddElement(nodeView);
    }
    /// <summary>
    /// 筛选可以互相连接的端口 当拖拽端口线时触发此方法
    /// 当用户从 startPort 拖出一条线时，告诉 GraphView 哪些 endPort 应该高亮显示（即兼容）
    /// 只允许Output端口连线
    /// </summary>
    /// <returns></returns>
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        List<Port> result = ports.Where(
            endPort => endPort.node != startPort.node && //自己不能连接到自己
            CheckCompatibleByPort(startPort, endPort)
            ).ToList();

        return result;
    }
    /// <summary>
    /// 检查两个端口之间是否可以连接
    /// </summary>
    /// <returns></returns>
    private bool CheckCompatibleByPort(Port startPort, Port endPort)
    {
        bool result = false;
        E_ConnectionType startConnectionType = (startPort.node as MapNodeView).GetConnectionTypeFromPort(startPort);
        E_ConnectionType endConnectionType = (endPort.node as MapNodeView).GetConnectionTypeFromPort(endPort);
        switch (startConnectionType)
        {
            case E_ConnectionType.TopOutput:
                if(endConnectionType == E_ConnectionType.BottomInput)
                    result = true;
                break;
            case E_ConnectionType.LeftOutput:
                if (endConnectionType == E_ConnectionType.RightInput)
                    result = true;
                break;
            case E_ConnectionType.RightOutput:
                if (endConnectionType == E_ConnectionType.LeftInput)
                    result = true;
                break;
            case E_ConnectionType.BottomOutput:
                if (endConnectionType == E_ConnectionType.TopInput)
                    result = true;
                break;
        }
        return result;
    }
}
