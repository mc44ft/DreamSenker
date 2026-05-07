# PlayArk 编辑器工具面试复习指南

> **项目概述**: DreamSenker 是一个基于 Unity 2022.3.62f2c1 的 2D 动作游戏项目，包含自研的图编辑器框架（GraphCore）、状态机系统（StateMachine）和对话系统（DialogueSystem）。
>
> **代码规模**: 共 3528 行代码，46 个 C# 文件
>
> **技术栈**: Unity Editor Extensions, UIElements, GraphView API, ScriptableObject, 反射, 序列化

---

## 目录

1. [架构设计总览](#架构设计总览)
2. [GraphCore 核心图系统](#graphcore-核心图系统)
3. [StateMachine 状态机系统](#statemachine-状态机系统)
4. [DialogueSystem 对话系统](#dialoguesystem-对话系统)
5. [关键技术点深度解析](#关键技术点深度解析)
6. [面试高频问题](#面试高频问题)
7. [扩展学习建议](#扩展学习建议)

---

## 架构设计总览

### 架构设计（基础-应用型）

```
GraphCore (基础层 - 通用图编辑器框架)
    ├── StateMachine (应用层 - 状态机)
    │       继承: StateMachine : GraphCoreGraph<State, StateTransitionEdge>
    │       继承: State : GraphCoreNode<GraphCorePort>
    │       继承: StateTransitionEdge : GraphCoreEdge
    │
    └── DialogueSystem (应用层 - 对话系统)
            继承: DialogueGraph : GraphCoreGraph<DialogueNodeBase, GraphCoreEdge>
            继承: DialogueNodeBase : GraphCoreNode<GraphCorePort>
```

> **核心思想**: GraphCore 是一个**通用的图编辑器框架**，提供节点管理、连线管理、序列化、编辑器 UI 等基础设施。StateMachine 和 DialogueSystem 是两个**并列的应用层系统**，各自继承 GraphCore 的泛型基类，实现不同的业务逻辑。这种设计使得添加新的图类型系统变得非常容易。

### 设计模式应用

| 设计模式 | 应用位置 | 代码示例 |
|---------|---------|---------|
| **策略模式** | GraphCoreView 泛型设计 | `GraphCoreView<TNodeView, TEdgeView>` |
| **工厂模式** | 节点/端口创建 | `CreatePortInstance()` |
| **观察者模式** | Undo/Redo 事件 | `Undo.undoRedoPerformed += OnUndoRedo` |
| **命令模式** | 撤销重做系统 | `Undo.RecordObject()` |
| **单例模式** | 管理器类 | `DialogueManager : SingletonMono<T>` |

### 核心技术栈

- **Unity Editor Extensions**: 自定义编辑器窗口、Inspector、Gizmos
- **UIElements/GraphView**: 现代化 UI 系统，替代 IMGUI
- **ScriptableObject**: 数据持久化和资源管理
- **反射 (Reflection)**: 动态菜单生成、类型查找
- **序列化**: ISerializationCallbackReceiver、SerializeReference

---

## GraphCore 核心图系统

### 1. 核心类设计

#### GraphCoreGraph - 图数据结构

```csharp
// 位置: Assets/PlayArk/GraphCore/GraphCoreGraph.cs
public class GraphCoreGraph<TNode, TEdge> : GraphCoreGraph, ISerializationCallbackReceiver
    where TNode : GraphCoreNode
    where TEdge : GraphCoreEdge, new()
{
    // 数据存储
    [SerializeField] protected List<TNode> _nodes = new();
    [SerializeField] protected List<TEdge> _edges = new();

    // 查找优化 - 懒加载字典
    protected readonly Dictionary<string, TNode> _nodeLookup = new();
    protected readonly Dictionary<string, TEdge> _edgeLookup = new();
    protected readonly Dictionary<string, TEdge> _portToEdgeLookup = new();

    // 懒加载机制
    private void RebuildLookups()
    {
        _nodeLookup.Clear();
        _edgeLookup.Clear();
        _portToEdgeLookup.Clear();

        foreach (var node in _nodes)
        {
            if (node == null) continue; // 防御性编程
            _nodeLookup[node.GetUniqueID()] = node;
        }

        foreach (var edge in _edges)
        {
            if (edge == null) continue;
            _edgeLookup[edge.UniqueID] = edge;
            _portToEdgeLookup[edge.RootPortID] = edge;
            _portToEdgeLookup[edge.ConnectionPortID] = edge;
        }
    }
}
```

**面试要点**:
- **为什么使用懒加载字典?** 字典不支持序列化，从磁盘加载后字典为空，需要重建
- **为什么要 Clear 后重建?** 提升鲁棒性，确保字典与列表数据一致，避免 NullReferenceException
- **防御性编程**: `if (node == null) continue` 处理手动删除资源或 Inspector 手动增加列表长度的情况

#### GraphCoreNode - 节点基类

```csharp
// 位置: Assets/PlayArk/GraphCore/GraphCoreNode.cs
public abstract class GraphCoreNode<TPort> : GraphCoreNode where TPort : GraphCorePort, new()
{
    [SerializeField] private string _title = "New Node";
    [HideInInspector, SerializeField] private string _uniqueID;
    [HideInInspector, SerializeField] private Vector2 _viewPosition;

    // 使用 SerializeReference 支持多态序列化
    [SerializeReference] protected List<TPort> _inputPorts = new List<TPort>();
    [SerializeReference] protected List<TPort> _outputPorts = new List<TPort>();

    // 工厂方法模式
    protected virtual TPort CreatePortInstance()
    {
        return new TPort();
    }

    public TPort CreatePort(E_PortDirection direction)
    {
        TPort portData = CreatePortInstance(); // 调用工厂方法
        portData.Initialize(Guid.NewGuid().ToString(), _uniqueID, direction);
        if (direction == E_PortDirection.Input)
            _inputPorts.Add(portData);
        else
            _outputPorts.Add(portData);
        return portData;
    }
}
```

**面试要点**:
- **SerializeReference 特性**: 允许序列化接口和抽象类的子类实例，支持多态
- **工厂方法模式**: `CreatePortInstance()` 允许子类重写创建特定类型的端口
- **GUID 作为唯一标识**: 使用 `Guid.NewGuid()` 确保节点和端口的全局唯一性

### 2. 编辑器扩展核心

#### GraphCoreEditor - 编辑器窗口

```csharp
// 位置: Assets/PlayArk/GraphCore/Editor/GraphCoreEditor.cs
public abstract class GraphCoreEditor : EditorWindow
{
    protected GraphCoreView _view;

    private void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        // 加载 UXML 布局文件
        VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            GetPath() + "GraphCoreEditor.uxml");
        visualTree.CloneTree(root);

        // 动态创建 GraphView
        VisualElement viewContainer = root.Q<VisualElement>("view-container");
        _view = CreateView();
        _view.style.flexGrow = 1;
        viewContainer.Add(_view);

        OnSelectionChange();
    }

    // 自动打开资源
    [OnOpenAsset(10)]
    private static bool OnGraphCoreOpened(int instanceID)
    {
        if(EditorUtility.InstanceIDToObject(instanceID) is GraphCoreGraph)
        {
            GetWindow<GraphCoreEditor>(false, "GraphCore");
            return true;
        }
        return false;
    }

    // 切换资源时刷新视图
    protected virtual void OnSelectionChange()
    {
        GraphCoreGraph graphCore = Selection.activeObject as GraphCoreGraph;
        if(graphCore != null)
        {
            _view.Refresh(graphCore);
        }
    }
}
```

**面试要点**:
- **OnOpenAsset 特性**: 双击资源时自动打开自定义编辑器窗口
- **UXML/USS 系统**: 类似 HTML/CSS 的 UI 布局方式，分离结构和样式
- **OnSelectionChange**: 监听 Project 和 Hierarchy 窗口的选择变化

#### GraphCoreView - GraphView 核心

```csharp
// 位置: Assets/PlayArk/GraphCore/Editor/GraphCoreView.cs
public abstract class GraphCoreView<TNodeView, TEdgeView> : GraphCoreView
    where TNodeView : GraphCoreNodeView<TEdgeView>, new()
    where TEdgeView : GraphCoreEdgeView, new()
{
    protected GraphCoreView()
    {
        // 加载样式
        StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
            GraphCoreEditor.GetPath() + "GraphCoreEditor.uss");
        styleSheets.Add(styleSheet);

        // 添加网格背景
        Insert(0, new GridBackground());

        // 添加操纵器
        this.AddManipulator(new ContentZoomer());      // 鼠标滚轮缩放
        this.AddManipulator(new ContentDragger());     // 中键拖拽画布
        this.AddManipulator(new SelectionDragger());   // 左键框选拖拽

        // 设置初始缩放
        viewTransform.scale = new Vector3(0.8f, 0.8f, 1f);

        // 订阅全局撤销/重做事件
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    public override void Refresh(GraphCoreGraph graphCore)
    {
        if (InterceptionGraph(graphCore)) return;
        _graphCore = graphCore;

        // 先取消订阅，避免误触发
        graphViewChanged -= OnGraphViewChanged;
        DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        // 重新绘制所有元素
        if(_graphCore != null)
        {
            foreach (var node in _graphCore.GetNodesInternal())
                DrawNode(node);
            foreach (var edge in _graphCore.GetEdgesInternal())
                DrawEdge(edge);
        }
    }
}
```

**面试要点**:
- **Manipulator 系统**: Unity GraphView 的交互扩展机制
- **graphViewChanged 事件**: 监听图的变化（添加/删除节点和连线）
- **数据层与表现层同步**: 通过事件机制保持一致性

### 3. 序列化机制

#### ISerializationCallbackReceiver 接口

```csharp
public void OnBeforeSerialize()
{
#if UNITY_EDITOR
    // 确保主资源已保存到磁盘
    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
        return;

    // 将子资源添加到主资源
    foreach (var node in _nodes)
    {
        if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(node)))
        {
            AssetDatabase.AddObjectToAsset(node, this);
        }
    }

    // 生成默认节点（自动修复机制）
    OnCreateDefaultNode();
#endif
}

public void OnAfterDeserialize() { }
```

**面试要点**:
- **序列化时机**: 手动保存 (Ctrl+S)、自动保存、打包、Play、代码编译后
- **子资源管理**: `AssetDatabase.AddObjectToAsset()` 将节点作为子资源保存到主资源
- **自动修复机制**: `OnCreateDefaultNode()` 确保资源完整性

### 4. 反射与动态菜单

```csharp
private void AppendMenuAction(ContextualMenuPopulateEvent evt, Vector2 mousePosition)
{
    var nodeTypes = GetMenuNodeType();
    foreach (var type in nodeTypes)
    {
        if (type.IsAbstract) continue; // 排除抽象类

        // 使用反射获取自定义特性
        var attribute = type.GetCustomAttribute<NodeMenuItemAttribute>();
        if(attribute != null && !string.IsNullOrEmpty(attribute.MenuTitle))
        {
            // 使用临时变量避免闭包问题
            var capturedType = type;
            evt.menu.AppendAction("Create " + attribute.MenuTitle,
                a => CreateNode(capturedType, mousePosition));
        }
    }
}
```

**面试要点**:
- **反射获取特性**: `GetCustomAttribute<T>()` 动态读取类的元数据
- **闭包陷阱**: 必须使用 `capturedType` 捕获循环变量，否则所有菜单项都指向最后一个类型
- **TypeCache**: Unity 提供的类型缓存系统，比直接反射更高效

---

## StateMachine 状态机系统

### 1. 状态机核心设计

#### StateMachine - 状态机主类

```csharp
// 位置: Assets/PlayArk/StateMachine/StateMachine.cs
[CreateAssetMenu(fileName = "StateMachine_", menuName = "PlayArk Assets/GraphCore/State Machine")]
public class StateMachine : GraphCoreGraph<State, StateTransitionEdge>
{
    [SerializeField, HideInInspector] private EntryState _entryState;
    [SerializeField, HideInInspector] private AnyState _anyState;
    private State _currentState;

    public void Bind(StateMachineController controller)
    {
        // 将控制器绑定到所有状态
        foreach (var node in _nodes)
            node.Bind(controller);
        foreach (var edge in _edges)
            edge.Bind(controller);
    }

    public void MachineEnter()
    {
        // 从 EntryState 的第一个转换开始
        TransitionToState(_entryState.Transitions[0].ConnectionNodeID);
    }

    public void LogicUpdate()
    {
        _currentState.LogicUpdate();
        _anyState.LogicUpdate(); // AnyState 每帧都检查
    }

    public void TransitionToState(string targetStateID)
    {
        _currentState?.Exit();
        _currentState = GetNodeByID(targetStateID);
        _currentState?.Enter();
    }
}
```

**面试要点**:
- **EntryState**: 状态机的入口，必须有且只有一个输出连接
- **AnyState**: 全局转换状态，每帧都检查转换条件，可以从任意状态转换
- **状态生命周期**: Enter → LogicUpdate (每帧) → Exit

#### State - 状态基类

```csharp
// 位置: Assets/PlayArk/StateMachine/State.cs
public class State : GraphCoreNode<GraphCorePort>
{
    private bool _started = false;
    protected StateMachineController _controller;
    private StateTransitionEdge[] _transitions;

    public void Bind(StateMachineController stateMachineController)
    {
        _controller = stateMachineController;
        // 填充节点的出度信息
        _transitions = _controller.StateMachine.GetEdges()
            .Where(edge => edge.RootNodeID == GetUniqueID())
            .ToArray();
    }

    public virtual void LogicUpdate()
    {
        CheckTransitions(); // 每帧检查转换条件
    }

    private void CheckTransitions()
    {
        foreach (var transition in _transitions)
        {
            bool success = transition.Check();
            if (success)
            {
                _controller.TransitionToState(transition.ConnectionNodeID);
            }
        }
    }
}
```

