# GraphCore新建对话图默认节点不刷新UI问题

## 现象

新建 `DialogueGraph` 资源时，默认入口节点已经能正常生成，且之前的 `Broken text PPtr` 报错已经消失。

但新建资源后，GraphCore 编辑器窗口不会立刻刷新 UI。需要重新选中资源、重新打开窗口，或者触发一次刷新后，默认节点才会显示出来。

## 当前判断

问题不在对话内容，也不在 `DialogueText`。

本质是数据生成时机和编辑器刷新时机不一致：

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

推荐修在 GraphCore 通用层，不只给 `DialogueGraphEditor` 打补丁。

核心目标：

```text
刷新 UI 前
-> 先保证图数据完整
-> 再绘制 GraphView
-> 用户立刻看到最新画布
```

### 1. 数据层提供明确入口

在 `GraphCoreGraph` 基类提供默认节点检查入口：

```csharp
public void EnsureDefaultNodes()
{
    OnCreateDefaultNode();
}
```

这个方法只负责数据完整性，不负责 UI。

具体职责：

- 调用 `OnCreateDefaultNode()` 创建缺失的默认节点。
- 确保新节点已经加入 `_nodes`。
- 确保新节点作为子资源挂到图资源下面。
- 如果真的创建或挂载了节点，标记图资源 dirty。

### 2. 编辑器层刷新前先保证数据完整

在 `GraphCoreEditor.OnSelectionChange()` 里，刷新画布前调用：

```csharp
GraphCoreGraph graphCore = Selection.activeObject as GraphCoreGraph;

if (graphCore != null)
{
    graphCore.EnsureDefaultNodes();
    _view.Refresh(graphCore);
}
```

这样职责是清楚的：

```text
GraphCoreGraph
-> 只管图数据完整

GraphCoreEditor / GraphCoreView
-> 只管编辑器 UI 刷新
```

### 3. 延迟刷新只作为兜底

首选同步流程：

```text
EnsureDefaultNodes()
-> _view.Refresh(graphCore)
```

如果验证后发现新建资源时 Unity 仍然有一帧资源稳定时序问题，再加一次延迟刷新：

```csharp
EditorApplication.delayCall += () =>
{
    _view.Refresh(graphCore);
};
```

不要一开始就加延迟刷新。

延迟刷新只能解决时序显示问题，不能替代数据完整性检查。

## 后续实现清单

- 在 `GraphCoreGraph` 暴露 `EnsureDefaultNodes()`。
- 把默认节点创建、子资源挂载、dirty 标记收进该方法。
- `OnBeforeSerialize()` 继续保留自动修复能力，但不碰 UI。
- `GraphCoreEditor.OnSelectionChange()` 在 `_view.Refresh(graphCore)` 前调用 `EnsureDefaultNodes()`。
- 若同步刷新仍不稳定，再在编辑器层加一次 `delayCall` 二次刷新。

## 验收标准

- 新建 `DialogueGraph` 后，GraphCore 编辑器窗口立刻显示入口节点。
- 重新选中资源、重新打开窗口后显示结果一致。
- 保存、脚本重载、进入 Play Mode 后不会重复生成默认节点。
- Unity Console 没有 `Broken text PPtr` 或默认节点 Missing 引用。
- 轻量验证即可：`rg` 查调用链，`git diff --check` 查格式，检查 Unity Console Error。

## 临时处理

当前问题影响不大，可以先不处理。

临时解决方式：

```text
重新选中对话图资源
或
重新打开 GraphCore 编辑器窗口
```
