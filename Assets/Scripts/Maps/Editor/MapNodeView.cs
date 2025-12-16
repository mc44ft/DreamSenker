
using MapSystem.Nodes;
using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class MapNodeView : Node
{
    private VisualElement m_topContainer;
    private VisualElement m_middleContainer;
    private VisualElement m_leftContainer;
    public VisualElement DataContainer;
    private VisualElement m_rightContainer;
    private VisualElement m_bottomContainer;

    //四个方向的端口
    public readonly Port TopInputPort;
    public readonly Port TopOutputPort;
    public readonly Port LeftInputPort;
    public readonly Port LeftOutputPort;
    public readonly Port RightInputPort;
    public readonly Port RightOutputPort;
    public readonly Port BottomInputPort;
    public readonly Port BottomOutputPort;

    public MapNode MapNode;
    private MapNodeView()
    {
        //清空默认的主容器
        mainContainer.Clear();

        //使用UXML配置元素
        var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/Maps/Editor/MapNodeViewUXML.uxml");
        VisualElement nodeLayout = treeAsset.Instantiate();
        mainContainer.Add(nodeLayout);

        //获取UXML中的自定义容器
        m_topContainer = nodeLayout.Q<VisualElement>("TopContainer");
        m_middleContainer = nodeLayout.Q<VisualElement>("MiddleContainer");
        m_leftContainer = m_middleContainer.Q<VisualElement>("LeftContainer");
        DataContainer = m_middleContainer.Q<VisualElement>("DataContainer");
        m_rightContainer = m_middleContainer.Q<VisualElement>("RightContainer");
        m_bottomContainer = nodeLayout.Q<VisualElement>("BottomContainer");

        //创建顶部输出端口
        TopOutputPort = CreatePort(Orientation.Vertical, Direction.Output, Color.cyan, m_topContainer);
        //创建顶部输入端口
        TopInputPort = CreatePort(Orientation.Vertical, Direction.Input, Color.red, m_topContainer);

        //创建左侧输出端口
        LeftOutputPort = CreatePort(Orientation.Horizontal, Direction.Output, Color.cyan, m_leftContainer);
        //创建左侧输入端口
        LeftInputPort = CreatePort(Orientation.Horizontal, Direction.Input, Color.red, m_leftContainer);

        //创建右侧输入端口
        RightInputPort = CreatePort(Orientation.Horizontal, Direction.Input, Color.red, m_rightContainer);
        //创建右侧输出端口
        RightOutputPort = CreatePort(Orientation.Horizontal, Direction.Output, Color.cyan, m_rightContainer);

        //创建底部输入端口
        BottomInputPort = CreatePort(Orientation.Vertical, Direction.Input, Color.red, m_bottomContainer);
        //创建底部输出端口
        BottomOutputPort = CreatePort(Orientation.Vertical, Direction.Output, Color.cyan, m_bottomContainer);

        //刷新
        RefreshExpandedState();
        RefreshPorts();
    }
    public MapNodeView(MapNode node) : this()
    {
        //viewDataKey是UI Toolkit中保存View信息的key
        //当给这个视图元素一个唯一key（这个唯一key一般用guid来记录）
        //在关闭编辑器窗口等时，UI Toolkit会自动的保存View数据
        //当下次加载该视图元素时 凭借该key可以恢复视图上的信息
        //但这个数据是保存在Library中 git不会记录、打包时不会一起带出、只相当于是个缓存记录
        //所以需要在Model中也存储一份位置数据 便于迁移view视图信息
        //重新打开编辑器也会触发构造函数，也会给这个viewDataKey重新赋值，
        //只不过UI Toolkit智能地发现了这个key已经有记录了，所以会直接覆盖所以保存的view数据
        
        MapNode = node;

        //设置视图位置 
        style.left = node.ViewData.Position.x;
        style.top = node.ViewData.Position.y;

        //如果已经有key 会覆盖所有的view信息
        viewDataKey = node.guid;

        //获取MapNode的序列化对象
        SerializedObject so = new SerializedObject(node);
        SerializedProperty mapDataProp = so.FindProperty("MapData");
        var mapDataField = new UnityEditor.UIElements.PropertyField(mapDataProp);
        DataContainer.Bind(so);
        DataContainer.Add(mapDataField);

        
    }
    private Port CreatePort(Orientation orientation, Direction direction, Color portColor, VisualElement container)
    {
        Port port = Port.Create<Edge>(orientation, direction, Port.Capacity.Single, typeof(bool));
        port.portName = "";
        port.portColor = portColor;

        if(direction == Direction.Input)
        {
            //移除输入端口可以主动连线的功能
            port.RemoveManipulator(port.edgeConnector);
        }
        container.Add(port);
        return port;
    }
    public E_ConnectionType GetConnectionTypeFromPort(Port port)
    {
        if (port == TopInputPort)
            return E_ConnectionType.TopInput;
        else if (port == LeftInputPort)
            return E_ConnectionType.LeftInput;
        else if (port == RightInputPort)
            return E_ConnectionType.RightInput;
        else if (port == BottomInputPort)
            return E_ConnectionType.BottomInput;
        else if (port == TopOutputPort)
            return E_ConnectionType.TopOutput;
        else if (port == LeftOutputPort)
            return E_ConnectionType.LeftOutput;
        else if (port == RightOutputPort)
            return E_ConnectionType.RightOutput;
        else if (port == BottomOutputPort)
            return E_ConnectionType.BottomOutput;
        else
        {
            throw new ArgumentOutOfRangeException(nameof(port), "未知的端口类型");
        }
    }
    public Port GetPortFromConnectionType(E_ConnectionType connectionType)
    {
        switch (connectionType)
        {
            case E_ConnectionType.TopInput:
                return TopInputPort;
            case E_ConnectionType.LeftInput:
                return LeftInputPort;
            case E_ConnectionType.RightInput:
                return RightInputPort;
            case E_ConnectionType.BottomInput:
                return BottomInputPort;
            case E_ConnectionType.TopOutput:
                return TopOutputPort;
            case E_ConnectionType.LeftOutput:
                return LeftOutputPort;
            case E_ConnectionType.RightOutput:
                return RightOutputPort;
            case E_ConnectionType.BottomOutput:
                return BottomOutputPort;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionType), "未知的端口连接类型");
        }
    }
    /// <summary>
    /// 拖动节点 松开鼠标时 此方法被调用
    /// 当节点被创建出来时 手动调用了
    /// </summary>
    /// <param name="newPos"></param>
    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);
        if(MapNode != null)
        {
            //使用节点的左上角坐标来定位节点
            //这里的xMin和yMin是节点的左上角坐标
            MapNode.ViewData.Position = new Vector2(newPos.xMin, newPos.yMin);
        }
    }
    /// <summary>
    /// 当该节点UI被选中时
    /// </summary>
    public override void OnSelected()
    {
        base.OnSelected();

        //将当前活动的资源设置为当前选中节点的Model
        //在Inspector窗口中显示
        Selection.activeObject = MapNode;
    }
}