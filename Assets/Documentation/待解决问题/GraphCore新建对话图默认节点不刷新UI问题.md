# GraphCore新建对话图默认节点不刷新UI问题

## 现象

新建 `DialogueGraph` 资源时，默认入口节点已经能正常生成，且之前的 `Broken text PPtr` 报错已经消失。

但新建资源后，GraphCore 编辑器窗口不会立刻刷新 UI。需要重新选中资源、重新打开窗口，或者触发一次刷新后，默认节点才会显示出来。

## 当前判断

问题不在对话内容，也不在 `DialogueText`。

当前更像是数据生成时机和编辑器刷新时机不一致：

```text
新建 DialogueGraph 资源
-> GraphCoreEditor.OnSelectionChange 刷新 UI
-> 此时默认节点可能还没稳定进入 _nodes / 子资源
-> GraphCoreGraph.OnBeforeSerialize 生成并挂载默认节点
-> 数据已经完整
-> 但 GraphView 没有再次刷新
```

## 已处理内容

`GraphCoreGraph.OnBeforeSerialize()` 中已经将：

```csharp
OnCreateDefaultNode();
```

移动到 `AssetDatabase.AddObjectToAsset(node, this)` 之前。

这样可以保证默认节点先创建，再被挂到图资源下面，避免 Unity 序列化时引用不存在的子资源。

## 不建议的做法

不建议在 `GraphCoreGraph.OnBeforeSerialize()` 最后强制刷新 UI。

原因：

- `GraphCoreGraph` 是数据层，不应该直接控制编辑器窗口。
- `OnBeforeSerialize()` 触发频率很高，包括保存、编译、Play、打包前。
- 在序列化回调里操作 UI，容易让数据层和编辑器表现层耦合。

## 推荐修复方向

后续可以把默认节点检查抽成明确方法：

```csharp
public void EnsureDefaultNodes()
{
    OnCreateDefaultNode();
}
```

然后在编辑器刷新前调用：

```csharp
GraphCoreGraph graphCore = Selection.activeObject as GraphCoreGraph;

if (graphCore != null)
{
    graphCore.EnsureDefaultNodes();
    _view.Refresh(graphCore);
}
```

这样做的目标是：

```text
刷新 UI 前
-> 先保证图数据完整
-> 再绘制 GraphView
```

## 待确认点

- `EnsureDefaultNodes()` 是否需要标记资源 dirty。
- `EnsureDefaultNodes()` 内部是否也要负责把新节点挂载为子资源。
- 新建资源时是否需要延迟一帧刷新 GraphView。

## 临时处理

当前问题影响不大，可以先不处理。

临时解决方式：

```text
重新选中对话图资源
或
重新打开 GraphCore 编辑器窗口
```
