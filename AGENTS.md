# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

# DreamSenker 项目开发指南

## 项目概述

DreamSenker 是一个基于 Unity 2022.3.62f2c1 的 2D 动作游戏项目，使用 URP 渲染管线。项目包含自研的图编辑器框架（GraphCore）、状态机系统（StateMachine）和对话系统（DialogueSystem）。

## 核心架构

### 1. Framework 框架层

位于 `Assets/Framework/`，提供基础设施：

- **单例模式**：`BaseManager<T>` 使用反射实现线程安全的单例，要求子类实现私有构造函数
- **资源管理**：`ResourcesManager` 统一管理资源加载（支持 Editor、Resources、AssetBundle、UnityWebRequest）
- **UI 管理**：`UIManager` + `PanelBase` 提供 UI 面板生命周期管理
- **事件中心**：`EventCenter` 实现解耦的事件通信
- **对象池**：`PoolManager` 管理游戏对象复用
- **数据持久化**：支持 JSON（LitJson）、二进制（Excel 数据）、PlayerPrefs

### 2. PlayArk 工具集

位于 `Assets/PlayArk/`，包含三个核心编辑器工具：

#### GraphCore（图编辑器核心）
- **基类**：`GraphCoreGraph<TNode, TEdge>` 提供节点和连线的泛型管理
- **序列化**：实现 `ISerializationCallbackReceiver`，自动将子资源保存到主资源
- **查找优化**：使用字典缓存（懒加载）加速节点/连线查找
- **依赖**：DialogueSystem 和 StateMachine 都基于此框架

#### StateMachine（状态机系统）
- **继承关系**：`StateMachine : GraphCoreGraph<State, StateTransitionEdge>`
- **特殊节点**：`EntryState`（入口）、`AnyState`（全局转换）
- **运行时克隆**：`Clone()` 方法深拷贝状态机实例，避免修改原始资源
- **条件系统**：`Condition` + `IPredicateEvaluator` 支持复杂的状态转换条件
- **绑定机制**：通过 `StateMachineController` 将状态机绑定到具体游戏对象

#### DialogueSystem（对话系统）
- **依赖**：依赖 Framework 和 GraphCore
- **节点类型**：Entry、Normal、Choice、ExternalUI、TaskPublish、CancelSave
- **UI 组件**：自定义 TextMeshPro 组件支持富文本和 Ruby 注音
- **运行时**：`DialogueManager` 管理对话流程

### 3. 游戏逻辑层

位于 `Assets/Scripts/`：

- **角色系统**：Player、Monster（Fox、Spider）使用状态机控制行为
- **数据管理**：`RunningDataManager` 管理运行时数据，ScriptableObject 存储配置
- **场景管理**：`MySceneManager` 处理场景加载和转换

## 开发规范

### Unity 环境
- **Unity 版本**：2022.3.62f2c1
- **渲染管线**：Universal Render Pipeline (URP) 14.0.12
- **API 兼容性**：.NET Standard 2.1
- **主要依赖**：Cinemachine 2.10.5、TextMeshPro 3.0.7、DOTween

### 分支策略
- **主分支**：`main`
- **当前开发分支**：`develop_organize`

### 代码约定

1. **注释保留**：
   - 重构或移动代码时，必须尽量保留原有注释
   - 只有当注释已经明显错误、过期或与新逻辑冲突时，才允许删除或改写
   - 大段重写前必须先确认注释是否需要迁移，不能为了省事直接删掉

2. **指令边界**：
   - 用户指令不明确时，必须先询问确认
   - 不允许在需求、范围或目标不清楚时擅自开始修改代码

## 协作与架构建议

用户不是零基础初学者。用户已有一年以上学习经验，正在通过重构自己的 Unity 项目提升架构能力，并希望项目能体现简历含金量。

给架构建议时必须做到：

1. **优先给优秀架构**：
   - 不要因为用户还在学习就只给低配方案
   - 可以提出更专业、更长期可维护的架构方向

2. **同时权衡成本**：
   - 说明复杂程度
   - 说明学习掌握的时间成本
   - 说明需要哪些前置知识
   - 说明内部涉及哪些设计模式
   - 说明可能带来的维护成本和过度设计风险

3. **提供多方案对比**：
   - 至少给出推荐方案和一个更简单的替代方案
   - 必要时补充更高级但成本更高的方案
   - 明确说明哪套方案最适合当前项目阶段

4. **兼顾教学目的**：
   - 讲架构时要解释关键知识点，帮助用户理解为什么这样设计
   - 不只给结论，也要讲清楚设计背后的取舍
   - 但仍要保持表达短、狠、准，避免废话

### Unity-MCP 集成

项目已集成 Unity-MCP (v0.61.0) 用于 AI 辅助开发：
- **安装方式**：通过 OpenUPM（已配置 scoped registry）
- **配置文件**：`.mcp.json` 在项目根目录
- **注意**：如需重新安装，优先使用 `.unitypackage` 方式避免依赖问题

## 常见开发任务

### 创建新的状态机

1. 在 Unity 编辑器：`Assets > Create > PlayArk Assets > GraphCore > State Machine`
2. 自动生成 EntryState 和 AnyState
3. 右键添加新状态节点（继承 `State` 或 `ActionState`）
4. 连接状态转换，配置 Condition

### 创建新的对话图

1. 在 Unity 编辑器：`Assets > Create > PlayArk Assets > DialogueSystem > Dialogue Graph`
2. 使用 DialogueNodeEntry 作为入口
3. 添加 DialogueNodeNormal（普通对话）或 DialogueNodeChoice（选择分支）

### 扩展 GraphCore

创建新的图编辑器系统：

```csharp
// 1. 定义节点类型
public class MyNode : GraphCoreNode { }

// 2. 定义连线类型
public class MyEdge : GraphCoreEdge { }

// 3. 定义图类型
[CreateAssetMenu(fileName = "MyGraph_", menuName = "MyTool/Graph")]
public class MyGraph : GraphCoreGraph<MyNode, MyEdge>
{
    protected override void OnCreateDefaultNode()
    {
        // 创建默认节点
    }
}
```

### 添加新的管理器

继承 `BaseManager<T>` 并实现私有构造函数：

```csharp
public class MyManager : BaseManager<MyManager>
{
    private MyManager() { } // 必须私有化构造函数

    // 使用 MyManager.Instance 访问
}
```

## 处理安装、配置问题的工作流程

### 基本原则

当遇到安装、配置等问题时，必须遵循以下流程：

1. **优先查阅官方文档**
   - 在执行任何操作前，先查找并阅读官方文档、README 或安装指南
   - 理解推荐的安装方式和注意事项
   - 了解已知问题和常见陷阱

2. **提供双路径方案**
   - **AI 自动执行路径**：列出 AI 将执行的具体步骤和命令
   - **用户手动操作路径**：列出用户可以自己操作的步骤

3. **分析利弊对比**
   - AI 自动执行的优势和风险
   - 用户手动操作的优势和风险
   - 考虑操作的可逆性、复杂度和出错风险

4. **让用户决策**
   - 如果手动操作更方便、更安全或更可控，明确告知用户
   - 询问用户是希望 AI 自动完成还是自己手动操作
   - 尊重用户的选择，不要擅自行动

### 何时优先推荐手动操作

以下情况应优先推荐用户手动操作：

- **图形界面操作更直观**：如 Unity 编辑器内的配置、导入资源等
- **需要可视化确认**：如选择文件、预览效果等
- **操作不可逆或有风险**：如删除文件、修改关键配置等
- **依赖外部下载**：如从 GitHub Releases 下载文件
- **涉及认证或登录**：如需要浏览器登录、输入密码等
- **首次尝试不确定的操作**：如新工具的安装、复杂的配置等

### 示例：Unity-MCP 安装

**官方文档推荐的安装方式：**
1. 下载 `.unitypackage` 文件直接导入（最简单）
2. 使用 `unity-mcp-cli install-plugin`（需要 CLI）
3. 通过 OpenUPM 添加（最复杂）

**AI 自动执行路径：**
- 安装 npm 包 `unity-mcp-cli`
- 运行 CLI 命令修改 `manifest.json`
- 配置 MCP 连接

**风险：**
- 可能遇到依赖解析问题
- 修改配置文件可能出错
- 难以处理图形界面的配置步骤

**用户手动操作路径：**
- 访问 GitHub Releases 页面
- 下载 `.unitypackage` 文件
- 拖入 Unity 编辑器导入
- 在 Unity 菜单中完成配置

**优势：**
- 更直观，可以看到导入过程
- 避免依赖问题
- 可以在导入时选择需要的组件

**结论：** 手动导入 `.unitypackage` 更简单可靠，应优先推荐。

---

## 开发规范

### Unity 环境
- **Unity 版本**：2022.3.62f2c1
- **渲染管线**：Universal Render Pipeline (URP) 14.0.12
- **API 兼容性**：.NET Standard 2.1
- **主要依赖**：Cinemachine 2.10.5、TextMeshPro 3.0.7、DOTween

### 分支策略
- **主分支**：`main`
- **当前开发分支**：`develop_organize`

### 代码约定

1. **命名空间**：
   - Framework 层不使用命名空间
   - PlayArk 工具使用 `PlayArk.{ToolName}` 命名空间
   - 编辑器代码使用嵌套的 `Editor` 命名空间

2. **ScriptableObject 资源**：
   - 使用 `[CreateAssetMenu]` 提供创建入口
   - 文件名使用下划线后缀（如 `StateMachine_`）
   - 保存在 `Assets/Config/` 对应子目录

3. **图编辑器资源**：
   - 继承 `GraphCoreGraph<TNode, TEdge>` 时必须实现 `OnCreateDefaultNode()` 创建默认节点
   - 子节点资源会自动保存到主资源中（通过 `OnBeforeSerialize`）
   - 使用 GUID 作为节点唯一标识

4. **状态机使用**：
   - 运行时必须先 `Clone()` 状态机实例
   - 通过 `Bind(controller)` 绑定到控制器
   - 调用 `MachineEnter()` 启动状态机

### Unity-MCP 集成

项目已集成 Unity-MCP (v0.61.0) 用于 AI 辅助开发：
- **安装方式**：通过 OpenUPM（已配置 scoped registry）
- **配置文件**：`.mcp.json` 在项目根目录
- **注意**：如需重新安装，优先使用 `.unitypackage` 方式避免依赖问题

---

## 经验教训记录

### 2026-03-28：Unity-MCP 安装

**问题：** 通过 Package Manager + OpenUPM 安装 Unity-MCP 时遇到 SignalR 依赖解析失败。

**解决方案：** 使用 `.unitypackage` 文件直接导入。

**教训：**
- 不要假设 CLI 工具就是最佳方案
- 对于 Unity 插件，`.unitypackage` 通常是最可靠的安装方式
- 在执行复杂操作前，必须先完整阅读官方文档
- 提供多种方案让用户选择，而不是直接执行
