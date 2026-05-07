# PlayArk 面试核心知识点速查表

## 一、条件系统 (Condition System)

### 合取范式 (CNF) 设计

```csharp
// 位置: Assets/PlayArk/StateMachine/Utilities/Condition.cs
// 外层 AND，内层 OR
[Serializable]
public class Condition
{
    [SerializeField] private Disjunction[] _and; // 所有项都满足才满足

    public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
    {
        foreach (var disjunction in _and)
        {
            if (!disjunction.Check(evaluators))
                return false;
        }
        return true;
    }
}

[Serializable]
public class Disjunction
{
    [SerializeField] private Predicate[] _or; // 有一项满足就满足

    public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
    {
        foreach (var predicate in _or)
        {
            if (predicate.Check(evaluators))
                return true;
        }
        return false;
    }
}

[Serializable]
public class Predicate
{
    [SerializeField] private EPredicate _predicate;  // 谓词主体
    [SerializeField] private string[] _parameters;   // 谓词参数
    [SerializeField] private bool _negate = false;   // 否定标志

    public bool Check(IEnumerable<IPredicateEvaluator> evaluators)
    {
        foreach (var evaluator in evaluators)
        {
            bool? result = evaluator.Evaluate(_predicate, _parameters);
            if (result == null) continue; // 不归该脚本管

            if (result == _negate)
                return false;
        }
        return true;
    }
}
```

**面试要点**:
- **CNF (合取范式)**: (A OR B) AND (C OR D) 的形式
- **三层结构**: Condition → Disjunction → Predicate
- **可空类型**: `bool?` 支持三种状态 (true/false/null)，null 表示该评估器不负责此条件
- **否定逻辑**: `_negate` 字段实现条件取反

### 谓词评估器接口

```csharp
public interface IPredicateEvaluator
{
    // 返回 bool? 可空类型
    // null: 不负责该条件
    // true/false: 条件判断结果
    bool? Evaluate(EPredicate predicate, string[] parameters);
}

public enum EPredicate
{
    KeyCodePressed,
    AnimOver,
    HorizontalNotZero,
    VerticalSpeedNotNegative,
    JumpCountNotZero,
    Grounded,
    TakeDamage,
    Dead,
}
```

**面试要点**:
- **责任链模式**: 多个评估器依次检查，找到负责的评估器
- **扩展性**: 添加新条件只需扩展枚举和实现评估器

---

## 二、克隆机制 (Clone Mechanism)

### 状态机深拷贝

```csharp
// 位置: Assets/PlayArk/StateMachine/StateMachine.cs
public StateMachine Clone()
{
    // Instantiate 是深拷贝
    StateMachine clone = Instantiate(this);

    // 清空引用（克隆体的引用还指向原对象）
    clone._nodes.Clear();
    clone._edges.Clear();
    clone._nodeLookup.Clear();
    clone._edgeLookup.Clear();
    clone._portToEdgeLookup.Clear();

    // 深拷贝所有节点和连线
    foreach (var state in _nodes)
        clone.AddNode(state.Clone());

    foreach (var edge in _edges)
        clone.AddEdge(edge.Clone());

    // 重新定位特殊状态
    clone._entryState = clone.GetNodeByID(_entryState.GetUniqueID()) as EntryState;
    clone._anyState = clone.GetNodeByID(_anyState.GetUniqueID()) as AnyState;

    return clone;
}
```

**面试要点**:
- **为什么需要克隆?** 运行时不能修改原始资源，需要独立实例
- **深拷贝步骤**: 1) Instantiate 主对象 2) 清空引用 3) 逐个克隆子对象
- **引用重定位**: 克隆后需要重新查找特殊节点的引用

### 使用流程

```csharp
// 位置: Assets/PlayArk/StateMachine/StateMachineController.cs
public class StateMachineController : MonoBehaviour
{
    [SerializeField] private StateMachine _stateMachine;

    private void Awake()
    {
        // 克隆一份运行时实例
        _stateMachine = _stateMachine.Clone();
    }

    private void Start()
    {
        _stateMachine.Bind(this);
        _stateMachine.MachineEnter();
    }

    protected virtual void Update()
    {
        _stateMachine.LogicUpdate();
    }
}
```

---

## 三、LazyEvent 懒事件机制

### 问题背景

状态机使用**轮询**检查条件，事件触发是**瞬时**的。如果事件在帧开始触发，状态机在帧末尾检查，就会错过事件。

### 解决方案

```csharp
// 位置: Assets/PlayArk/StateMachine/Utilities/LazyEvent.cs
[Serializable]
public class LazyEvent : UnityEvent
{
    private const float _flagResetTime = 0.001f; // 维持 1ms
    private bool _wasInvoked = false;

    public void StratInvoke()
    {
        MonoManager.Instance.StartCoroutine(Invoke());
    }

    private new IEnumerator Invoke()
    {
        base.Invoke();
        _wasInvoked = true;
        yield return new WaitForSeconds(_flagResetTime);
        _wasInvoked = false; // 1ms 后重置标志
    }

    public bool WasInvoked() => _wasInvoked;
}
```

**面试要点**:
- **时序问题**: 轮询与事件的时序不同步
- **解决思路**: 让事件维持一个极短的时间窗口 (1ms)
- **使用场景**: 受伤、跳跃等瞬时事件

---

## 四、ActionState 动作状态

### 设计思想

将状态的行为抽象为可配置的 Action 列表，支持在不同生命周期执行不同动作。

```csharp
// 位置: Assets/PlayArk/StateMachine/ActionState.cs
[NodeMenuItem("Action State")]
public class ActionState : State
{
    [SerializeField] private ActionData[] _onEnterActions;
    [SerializeField] private ActionData[] _onLogicUpdateActions;
    [SerializeField] private ActionData[] _onPhysicsUpdateActions;
    [SerializeField] private ActionData[] _onExitActions;

    public override void Enter()
    {
        base.Enter();
        DoActions(_onEnterActions);
    }

    private void DoActions(ActionData[] actions)
    {
        foreach (var actionSender in _controller.GetComponents<IAction>())
        {
            foreach (ActionData action in actions)
            {
                actionSender.DoAction(action.action, action.parameters);
            }
        }
    }

    [Serializable]
    private class ActionData
    {
        public EAction action;      // 要执行的逻辑段
        public string[] parameters; // 参数（如动画名）
    }
}
```

**面试要点**:
- **数据驱动**: 通过配置而非代码定义状态行为
- **IAction 接口**: 组件实现接口来响应动作
- **灵活性**: 同一个 ActionState 可以配置不同的动作组合

---

## 五、对话系统核心

### 节点执行流程

```csharp
// 位置: Assets/PlayArk/DialogueSystem/Scripts/GraphEditor/Data/Nodes/DialogueNodeBase.cs
public abstract class DialogueNodeBase : GraphCoreNode<GraphCorePort>
{
    public E_NodeState CurrentState { get; protected set; }
    protected Action<bool> _onFinished;

    public void Execute()
    {
        if (CurrentState != E_NodeState.Wating) return;
        CurrentState = E_NodeState.Executing;
        OnExecute(); // 子类实现具体逻辑
    }

    public void OnFinished()
    {
        Finished(); // 清理工作
        CurrentState = E_NodeState.Finished;
        _onFinished?.Invoke(true); // 通知图执行下一个节点
    }

    protected abstract void OnExecute();
    protected abstract void Finished();
}

public enum E_NodeState
{
    Wating,
    Executing,
    Finished
}
```

### DialogueNodeChoice 选择节点

```csharp
// 位置: Assets/PlayArk/DialogueSystem/Scripts/GraphEditor/Data/Nodes/DialogueNodeChoice.cs
[NodeMenuItem("DialogueNodeChoice")]
public class DialogueNodeChoice : DialogueNodeBase
{
    [SerializeField] private int _defaultSelectIndex = 0;
    private int _resultIndex; // 玩家选择的索引

    protected override void OnExecute()
    {
        // 获取所有选项数据
        ChoiceData[] choiceDatas = _outputPorts
            .Select(port => (port as DialogueChoicePort).ChoiceData)
            .ToArray();

        // 显示选项框
        DialogueManager.Instance.ShowDialogueChoicesSection(choiceDatas, _defaultSelectIndex);

        // 监听玩家选择
        EventCenter.Instance.AddEventListener<IntEventArgs>(
            E_EventType.Dialogue_ChoiceClick, OnChoiceClick);
    }

    private void OnChoiceClick(object eventSender, IntEventArgs args)
    {
        _resultIndex = args.Value;
        OnFinished();
    }

    // 根据玩家选择返回对应的端口ID
    public override string GetNextPortID()
    {
        return _outputPorts[_resultIndex].GetUniqueID();
    }

    // 使用自定义端口类型
    protected override GraphCorePort CreatePortInstance()
    {
        return new DialogueChoicePort();
    }
}

[Serializable]
public class DialogueChoicePort : GraphCorePort
{
    public ChoiceData ChoiceData; // 选项数据
}
```

**面试要点**:
- **动态分支**: 根据玩家选择返回不同的端口ID
- **自定义端口**: 重写 `CreatePortInstance()` 创建特殊端口类型
- **事件驱动**: 通过事件系统解耦UI和逻辑

---

## 六、文本处理系统

### Ruby 注音预处理

```csharp
// 位置: Assets/PlayArk/DialogueSystem/Scripts/UI/OverrideUI/DialogueTextPreprocessor.cs
public class DialogueTextPreprocessor : ITextPreprocessor
{
    public Dictionary<int, float> IntervalDict = new Dictionary<int, float>();
    public List<RubyData> RubyDataList = new List<RubyData>();

    public string PreprocessText(string text)
    {
        // 处理标签:
        // <数字> - 停顿时间
        // <r=注音内容> - 开始注音
        // </r> - 结束注音

        string pattern = "<.*?>";
        Match match = Regex.Match(processingText, pattern);

        while (match.Success)
        {
            string label = match.Value.Substring(1, match.Value.Length - 2);

            if (float.TryParse(label, out float result))
            {
                // 停顿时间
                IntervalDict[match.Index - 1] = result;
            }
            else if (Regex.IsMatch(label, "^r=.+"))
            {
                // 注音开始
                RubyDataList.Add(new RubyData(match.Index, label.Substring(2)));
            }
            else if (Regex.IsMatch(label, "/r"))
            {
                // 注音结束
                if(RubyDataList.Count > 0)
                    RubyDataList[RubyDataList.Count - 1].EndIndex = match.Index - 1;
            }

            match = match.NextMatch();
        }

        return processingText;
    }
}
```

### 逐字打印效果

```csharp
// 位置: Assets/PlayArk/DialogueSystem/Scripts/UI/OverrideUI/DialogueText.cs
public class DialogueText : TextMeshProUGUI
{
    private IEnumerator TypingCoroutine(float fadingDuration)
    {
        _currentTypingIndex = 0;
        while (_currentTypingIndex < m_characterCount)
        {
            // 逐字淡入
            StartCoroutine(FadeInCharacterToOut(_currentTypingIndex));

            // 检查是否有自定义停顿时间
            if (SelfTextPreprocessor.IntervalDict.TryGetValue(_currentTypingIndex, out float intervalTime))
                yield return new WaitForSecondsRealtime(intervalTime);
            else
                yield return new WaitForSecondsRealtime(_defaultIntervalTime);

            _currentTypingIndex++;
        }

        // 打印完成
        EventCenter.Instance.EventTrigger(E_EventType.Dialogue_PrintShowed, this, new EmptyEventArgs());
    }

    private IEnumerator FadeInCharacterToOut(int index)
    {
        // 修改顶点颜色实现淡入效果
        float elapsedTime = 0f;
        while (elapsedTime < _fadingDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0, 255, elapsedTime / _fadingDuration);
            SetCharacterAlpha(index, (byte)alpha);
            yield return null;
        }
        SetCharacterAlpha(index, 255);
    }
}
```

**面试要点**:
- **正则表达式**: 解析富文本标签
- **顶点颜色控制**: 通过修改 TextMeshPro 的顶点颜色实现淡入效果
- **自定义停顿**: 支持在特定位置插入停顿时间

---

## 七、Undo/Redo 系统

### 记录操作

```csharp
// 创建新对象
Undo.RegisterCreatedObjectUndo(node, "你刚刚创建了一个GraphCoreNode");

// 记录对象状态
Undo.RecordObject(_graphCore, "你刚刚添加了一个GraphCoreNode");

// 销毁对象
Undo.DestroyObjectImmediate(nodeView.CoreNode);
```

### 监听撤销/重做

```csharp
protected GraphCoreView()
{
    // 订阅全局撤销/重做事件
    Undo.undoRedoPerformed += OnUndoRedo;
}

private void OnUndoRedo()
{
    Refresh(_graphCore); // 刷新视图
}
```

**面试要点**:
- **三种操作**: RegisterCreatedObjectUndo (创建)、RecordObject (修改)、DestroyObjectImmediate (删除)
- **自动标记脏**: Undo 系统会自动标记资源为脏，触发保存
- **全局事件**: `Undo.undoRedoPerformed` 在任何撤销/重做操作后触发

---

## 八、端口连接规则

```csharp
// 位置: Assets/PlayArk/GraphCore/Editor/GraphCoreView.cs
public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
{
    List<Port> compatiblePorts = new List<Port>();

    foreach (var endPort in ports)
    {
        // 1. 排除被禁用的端口
        if (!endPort.enabledSelf)
            continue;

        // 2. 排除方向相同的端口
        if (endPort.direction == startPort.direction)
            continue;

        // 3. 排除自身节点的端口
        if ((endPort.userData as GraphCorePort)?.GetSeleNodeID() ==
            (startPort.userData as GraphCorePort)?.GetSeleNodeID())
            continue;

        // 4. 排除已经连接的端口
        if (AreConnected(startPort, endPort))
            continue;

        compatiblePorts.Add(endPort);
    }

    return compatiblePorts;
}
```

**面试要点**:
- **四重过滤**: 禁用、方向、自身、已连接
- **userData**: 端口的 userData 存储 GraphCorePort 数据对象
- **智能连接**: Unity GraphView 会自动高亮兼容端口

---

## 九、坐标转换

### 画布坐标与屏幕坐标转换

```csharp
// 位置: Assets/PlayArk/GraphCore/Editor/GraphCoreView.cs
public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
{
    // 解决画布缩放拖拽和屏幕尺寸不同步的问题
    Vector2 mousePosition =
        viewTransform.matrix.    // 当前画布的变换矩阵
        inverse.                 // 逆矩阵
        MultiplyPoint(evt.mousePosition); // 矩阵计算

    AppendMenuAction(evt, mousePosition);
}
```

**面试要点**:
- **变换矩阵**: `viewTransform.matrix` 包含缩放、平移、旋转信息
- **逆矩阵**: 将屏幕坐标转换为画布坐标
- **MultiplyPoint**: 矩阵乘法实现坐标变换

---

## 十、关键技术总结

### 1. 序列化相关

| 特性/接口 | 用途 | 示例 |
|----------|------|------|
| `[SerializeField]` | 序列化私有字段 | `[SerializeField] private string _title` |
| `[SerializeReference]` | 序列化多态类型 | `[SerializeReference] protected List<TPort> _inputPorts` |
| `[HideInInspector]` | 隐藏 Inspector 显示 | `[HideInInspector, SerializeField] private string _uniqueID` |
| `ISerializationCallbackReceiver` | 序列化回调 | `OnBeforeSerialize()` / `OnAfterDeserialize()` |

### 2. 编辑器扩展

| 特性/类 | 用途 | 示例 |
|--------|------|------|
| `[OnOpenAsset]` | 双击资源回调 | `[OnOpenAsset(10)]` |
| `[CreateAssetMenu]` | 创建资源菜单 | `[CreateAssetMenu(fileName = "StateMachine_")]` |
| `EditorWindow` | 自定义编辑器窗口 | `public class GraphCoreEditor : EditorWindow` |
| `GraphView` | 图编辑器视图 | `public class GraphCoreView : GraphView` |
| `Undo` | 撤销/重做系统 | `Undo.RecordObject()` |

### 3. UIElements

| 类/方法 | 用途 | 示例 |
|--------|------|------|
| `VisualTreeAsset` | UXML 布局资源 | `visualTree.CloneTree(root)` |
| `StyleSheet` | USS 样式资源 | `styleSheets.Add(styleSheet)` |
| `Q<T>()` | 查询 UI 元素 | `this.Q<Label>("title-label")` |
| `AddManipulator()` | 添加交互操纵器 | `AddManipulator(new ContentZoomer())` |

### 4. 反射

| 方法 | 用途 | 示例 |
|-----|------|------|
| `GetCustomAttribute<T>()` | 获取自定义特性 | `type.GetCustomAttribute<NodeMenuItemAttribute>()` |
| `TypeCache.GetTypesDerivedFrom<T>()` | 获取派生类型 | `TypeCache.GetTypesDerivedFrom<DialogueNodeBase>()` |
| `Guid.NewGuid()` | 生成唯一标识 | `Guid.NewGuid().ToString()` |

---

## 面试常见问题

### Q1: 为什么字典需要懒加载？

**A**: 字典不支持 Unity 序列化，从磁盘加载资源后字典为空。通过懒加载在首次使用时重建字典，确保数据一致性。

### Q2: SerializeReference 和 SerializeField 的区别？

**A**:
- `SerializeField`: 只能序列化具体类型，不支持多态
- `SerializeReference`: 支持序列化接口和抽象类的子类实例，保留类型信息

### Q3: 为什么需要克隆状态机？

**A**: 运行时不能修改原始资源（ScriptableObject），否则会污染资源文件。克隆创建独立实例，每个游戏对象有自己的状态机副本。

### Q4: 闭包陷阱是什么？

**A**: 在循环中使用 Lambda 表达式时，如果直接捕获循环变量，所有 Lambda 都会引用最后一个值。必须使用临时变量捕获当前值。

```csharp
// 错误
foreach (var type in types)
    evt.menu.AppendAction("Create", a => CreateNode(type)); // 所有都是最后一个 type

// 正确
foreach (var type in types)
{
    var capturedType = type; // 捕获当前值
    evt.menu.AppendAction("Create", a => CreateNode(capturedType));
}
```

### Q5: AnyState 和普通 State 的区别？

**A**:
- **普通 State**: 只在当前状态时检查转换条件
- **AnyState**: 每帧都检查，可以从任意状态转换，用于全局转换（如死亡、受伤）

### Q6: 为什么需要 LazyEvent？

**A**: 状态机使用轮询检查条件，事件触发是瞬时的。LazyEvent 让事件维持 1ms，确保状态机不会错过事件。

### Q7: OnBeforeSerialize 什么时候触发？

**A**:
1. 手动保存 (Ctrl+S)
2. 自动保存
3. 打包
4. 进入 Play 模式
5. 代码编译后

### Q8: 如何实现自定义端口类型？

**A**: 重写节点的 `CreatePortInstance()` 方法，返回自定义端口类型。

```csharp
protected override GraphCorePort CreatePortInstance()
{
    return new DialogueChoicePort(); // 自定义端口
}
```

---

## 快速学习路径

### 第一阶段：基础理解 (1-2天)
1. 理解三层架构：GraphCore → StateMachine → DialogueSystem
2. 掌握核心类：GraphCoreGraph、GraphCoreNode、GraphCoreEdge
3. 理解序列化机制：ISerializationCallbackReceiver

### 第二阶段：编辑器扩展 (2-3天)
1. 学习 UIElements/GraphView API
2. 理解 Undo/Redo 系统
3. 掌握反射和动态菜单

### 第三阶段：状态机系统 (2-3天)
1. 理解条件系统 (CNF)
2. 掌握克隆机制
3. 理解 LazyEvent 和 ActionState

### 第四阶段：对话系统 (1-2天)
1. 理解节点执行流程
2. 掌握文本处理系统
3. 理解事件驱动架构

### 推荐学习资源
- Unity 官方文档：UIElements、GraphView
- 设计模式：工厂模式、策略模式、观察者模式
- C# 高级特性：泛型、反射、特性、可空类型

---

**祝你面试顺利！** 🎉
