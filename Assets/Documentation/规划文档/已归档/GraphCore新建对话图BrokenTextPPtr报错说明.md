# GraphCore新建对话图Broken text PPtr报错说明
## 结论

这个报错不是对话文本坏了，也不是运行时对话逻辑坏了。

真正问题是：`DialogueGraph` 创建默认入口节点的时机不对，不应该在 Unity 的序列化回调里创建 `DialogueNodeEntry` 子资源，
导致 Unity 从内存写入资产到硬盘时，Unity已经确定本轮要写哪些资源块，但添加的子资源不在本轮写入列表中。

后面资源又能正常编辑，是因为 Unity 后续又触发导入、保存或刷新，把子资源补进去了。也就是说，这个问题不是完全不可恢复的坏档，而是创建阶段的资产写入时序不稳定。

## 点击创建资源后发生了什么

这里不是 Unity 官方逐行源码顺序，而是按 Unity 资产创建机制和当前项目代码推出来的实际流程。

当你在 Project 窗口里点击：

```text
Create
-> PlayArk Assets
-> GraphCore
-> DialogueGraph
```

Unity 大致经历的是：

```text
1. 根据 [CreateAssetMenu] 找到 DialogueGraph 类型
2. 在内存里 CreateInstance<DialogueGraph>()
3. Project 窗口进入命名状态，等待你输入资源名
4. 你按回车确认名字
5. Unity 准备在磁盘上创建 .asset 文件
6. Unity 开始序列化 DialogueGraph 主资源
7. 触发 DialogueGraph 继承来的 OnBeforeSerialize()
8. 项目代码在 OnBeforeSerialize() 里才创建默认 Entry 节点
9. 项目代码把 Entry 节点写进 _nodeEntry 和 _nodes
10. 项目代码再尝试 AssetDatabase.AddObjectToAsset(entry, graph)
11. Unity 完成本轮 .asset 写入或导入检查
12. Unity 发现主资源里已经有 entry 的 fileID 引用
13. 对应的 entry 子资源对象块还没有被写入或还没有被本轮导入识别
14. Unity 报 Broken text PPtr
```

第 6 步的关键点：

```text
Unity 开始序列化 DialogueGraph 主资源时，本轮要写哪些资源块已经确定。
所以在序列化过程中再添加子资源，确实添加进去了，但不在本轮写入列表中。
```

核心冲突在第 6 步到第 10 步。

正常设计应该是：

```text
序列化开始前
-> 图资源已经完整
-> Entry 节点已经是 graph 的子资源
-> _nodeEntry 和 _nodes 指向的是已挂载的子资源
```

现在的设计是：

```text
序列化开始时
-> 图资源还不完整
-> OnBeforeSerialize 临时创建 Entry 节点
-> OnBeforeSerialize 临时挂子资源
```

这就像已经开始打印文件了，才往文件里加一页目录。大部分时候 Unity 后面会补好，但第一轮检查很容易看到半成品。

## 另一个隐患

文件：

```text
Assets/PlayArk/GraphCore/Editor/GraphCoreView.cs
```

普通节点创建时也有类似问题：

```csharp
private void CreateNode(Type nodeType, Vector2 mousePosition)
{
    GraphCoreNode node = _graphCore.CreateNodeInternal(nodeType, mousePosition);

    Undo.RegisterCreatedObjectUndo(node, "你刚刚创建了一个GraphCoreNode");
    Undo.RecordObject(_graphCore, "你刚刚添加了一个GraphCoreNode");
    _graphCore.AddNodeInternal(node);

    DrawNode(node);
}
```

这里也只是：

```text
创建节点
-> 加进 _nodes
-> 画到 UI 上
```

但没有立刻把节点作为子资源挂进图资产。

现在它依赖 `OnBeforeSerialize()` 后补：

```text
等下一次保存或序列化
-> 再 AddObjectToAsset
```

这套逻辑能跑，但不稳。

## 推荐修复方向

应该把“保证图资源完整”从 `OnBeforeSerialize()` 里拆出来，变成一个明确的编辑器流程。

推荐结构：

```text
创建 DialogueGraph 资源
-> 创建默认 Entry 节点
-> AddObjectToAsset(entry, graph)
-> _nodeEntry = entry
-> _nodes.Add(entry)
-> EditorUtility.SetDirty(graph)
-> AssetDatabase.SaveAssets()
-> 刷新 GraphView
```

核心原则：

```text
先让资产结构完整
再让 Unity 保存
再让编辑器 UI 刷新
```

## 实现方案：自定义资产创建菜单

不要只依赖 `[CreateAssetMenu]`。

改成自定义创建入口：

```text
Assets/Create/PlayArk Assets/GraphCore/DialogueGraph
```

创建时完整执行：

```text
CreateAsset
-> Create default nodes
-> AddObjectToAsset
-> SetDirty
-> SaveAssets
```
在这个项目里，`DialogueGraph` 主资源会引用多个节点子资源。

