# MapSystem SO 重构规划

**制定日期**: 2026-04-23
**目标**: 移除 `MapGraph` 图编辑器依赖，将关卡存储和切换统一收敛为 ScriptableObject 配置。

---

## 一、重构目标

当前 `MapSystem` 的问题不是功能不够，而是结构混乱：

- 读档走 `SaveMapSceneName`
- 场景切换走 `CurrentMapNodeGuid / PreviousMapNodeGuid`
- 地图配置依赖 `MapGraph -> MapNode -> MapData`
- `MapGraph_.asset` 当前几乎没有有效联通数据，运行时价值很低

这套设计同时维护了两种地图身份，复杂度高，维护收益低。

本次重构目标：

1. 用 **每关一个 `MapDefinitionSO`** 取代 `MapGraph / MapNode / MapData`
2. 用 **连接表驱动** 取代图节点和边的跳转查询
3. 用 **统一的地图标识字段** 取代 `sceneName + nodeGuid` 混用
4. 先切运行时，再删除旧图系统

---

## 二、最终结构

### 2.1 地图配置

新增 `MapDefinitionSO`，每个地图一个资源。

建议字段：

- `MapId`
  - 地图唯一标识，字符串
  - 运行时、存档都只认它
- `SceneName`
  - 对应 Unity 场景名
- `PlayerShadowDarknessStrength`
  - 玩家阴影强度

结论：

- `MapDefinitionSO` 只描述地图自己
- 不在地图 SO 上直接存“东南西北连到哪”
- 地图之间的联通关系统一放到连接表里

### 2.2 连接数据

新增 `MapConnectionDatabaseSO`，统一存所有地图连接。

建议结构：

#### `MapEndpointData`

- `MapId`
  - 该端点属于哪张地图
- `PointId`
  - 该端点在该地图里的点位标识

#### `MapConnectionData`

- `ConnectionId`
  - 一条连接的唯一标识
- `EndA`
  - 一端点数据
- `EndB`
  - 另一端点数据

#### `MapConnectionDatabaseSO`

- `Connections`
  - 所有 `MapConnectionData` 列表

说明：

- 运行时不再按方向查目标地图
- 改为通过 `ConnectionId + 当前 PointId` 找连接的另一端
- “出口/入口”不是稳定数据，不单独存

### 2.3 场景点位组件

新增场景切图点组件 `MapTransitionPoint`。

建议字段：

- `ConnectionId`
  - 当前点属于哪条连接
- `PointId`
  - 当前点在本地图中的点位标识
- `LoadingSprite`
- `LoadingTime`

场景中的出生点仍然需要稳定标识。

可以保留现有出生点体系，或新增单独的 `MapSpawnPoint`。  
但无论用哪种形式，运行时最终都必须支持：

- 根据目标 `PointId` 找到场景中的落点位置

### 2.4 GameManager 持有的数据

`GameManager` 改为持有：

- `InitialMap`
  - 新游戏默认地图
- `MapDefinitions`
  - 全量地图配置列表
- `ConnectionDatabase`
  - 全量地图连接数据

运行时通过 `MapId` 或 `SceneName` 在该列表中查找目标地图，不再依赖 `MapGraph`

### 2.5 存档结构

`GameSaveData` 收敛为一套地图身份字段：

- `SavePointID`
- `SaveMapId`
- `CurrentMapId`
- `PreviousMapId`

删除：

- `SaveMapSceneName`
- `CurrentMapNodeGuid`
- `PreviousMapNodeGuid`

结论：

- 不兼容旧档
- 重构后直接走新档结构，别搞双轨兼容，那是给自己埋雷

---

## 三、运行时改造方案

### 3.1 初始化游戏

当前流程：

- 从 `GameSaveData.SaveMapSceneName` 加载场景
- 再从 `MapGraph` 反查当前节点数据

重构后流程：

1. 先根据 `GameSaveData.SaveMapId` 找到 `MapDefinitionSO`
2. 通过其 `SceneName` 加载场景
3. 玩家初始化后，直接使用当前 `MapDefinitionSO.PlayerShadowDarknessStrength`
4. 更新 `CurrentMapId`

### 3.2 普通出口切图

当前流程：

- 由旧图节点和边推导目标地图

重构后流程：

1. 玩家进入某个 `MapTransitionPoint`
2. 运行时读取该点位的：
   - `ConnectionId`
   - `PointId`
3. 在 `MapConnectionDatabaseSO` 中找到对应连接
4. 判断当前点是 `EndA` 还是 `EndB`
5. 取另一端作为目标端点
6. 根据目标端点的 `MapId` 找到目标 `MapDefinitionSO`
7. 更新：
   - `PreviousMapId`
   - `CurrentMapId`
8. 加载目标场景
9. 根据目标端点的 `PointId` 找到落点位置
10. 设置玩家位置与阴影参数

关键点：

- 不再依赖方向换算
- 切图逻辑由“旧图结构驱动”改成“连接驱动”

### 3.3 传送门切图

当前 `MapTeleport` 使用 `E_MapSceneName`。

重构后改为：

- 也统一接入连接表体系，或保留为特殊直达逻辑
- 如果接入连接表，则传送门本质也是一个 `MapTransitionPoint`
- 如果保留特殊逻辑，也必须最终落到：
  - 目标地图 `MapId`
  - 目标落点 `PointId`

好处：

- 不再需要 `GetSceneNameFromEnum`
- 不再多维护一套地图枚举映射

### 3.4 特殊地图逻辑

`GameManager.UpdateMapFromGameSaveData()` 当前基于场景名和旧图数据判断：

- `CaveMap`
- `MirrorMap1`
- `MirrorMap2`

重构后保留现有行为，但数据来源改成：

- 当前地图：`CurrentMapId`
- 上一张地图：`PreviousMapId`
- 或从当前 `MapDefinitionSO.SceneName` 判断

原则：

- **行为不变**
- **只换数据来源**

---

## 四、迁移步骤

### Phase 1：建立新数据结构

目标：把 `MapDefinitionSO`、连接表和新存档字段先立起来。

工作项：

1. 新建 `MapDefinitionSO`
2. 为现有地图分别创建资源
   - `CampMap`
   - `MagicMap`
   - `CaveMap`
   - `FoxMap`
   - `MirrorMap1`
   - `MirrorMap2`
3. 新建 `MapConnectionDatabaseSO`
4. 配置所有地图连接数据
   - 每条连接有唯一 `ConnectionId`
   - 每端记录 `MapId + PointId`
5. 给 `GameManager` 增加：
   - `InitialMap`
   - `MapDefinitions`
   - `ConnectionDatabase`
6. 给 `GameSaveData` 增加：
   - `SaveMapId`
   - `CurrentMapId`
   - `PreviousMapId`

### Phase 2：切运行时主链路

目标：让游戏运行时不再依赖 `MapGraph`。

工作项：

1. 改 `InitializeGame()`
2. 改 `ChangeMap(...)`，让它基于 `ConnectionId + PointId` 查另一端
3. 改 `TeleportMap(...)`
4. 改 `UpdateMapFromGameSaveData()`
5. 改玩家阴影初始化逻辑
6. 改地图查找逻辑，统一走 `MapDefinitionSO`
7. 改落点查找逻辑，统一走 `PointId`

完成标准：

- `GameManager` 运行时不再调用任何 `MapGraph.GetNodeByID / GetEdgeByPortID / GetNodes`
- `GameManager` 运行时不再依赖方向字段推导目标地图

### Phase 3：切场景与配置引用

目标：让场景和配置资源彻底引用新数据。

工作项：

1. 新增或改造场景切图点组件 `MapTransitionPoint`
2. 让每个切图点配置：
   - `ConnectionId`
   - `PointId`
3. `MapTeleport` 从 `E_MapSceneName` 改成新体系
4. `SelectMap` 场景中的 `GameManager` 序列化数据切到：
   - `InitialMap`
   - `MapDefinitions`
   - `ConnectionDatabase`
5. `GameConfig_main.asset` 默认存档改成新字段

### Phase 4：清理旧系统

目标：彻底删掉废结构。

删除项：

- `MapGraph`
- `MapNode`
- `MapGraphPort`
- `MapData`
- `MapGraphEditor`
- `MapGraphView`
- `MapGraph_.asset`
- `GameManager` 上的 `MapGraph` 字段
- `GameSaveData` 里的 guid 字段
- `E_MapSceneName` 及其相关映射（若无其他引用）

原则：

- **先确保运行时切完**
- **再删旧结构**
- 别反过来，反过来就是纯找死

---

## 五、测试清单

### 5.1 新游戏流程

- 新游戏能正常进入初始地图
- 玩家出生点正确
- 阴影强度正确

### 5.2 普通出口切换

验证项：

- 连接表能正确找到另一端
- 目标地图正确
- 玩家落在目标 `PointId` 对应位置
- 当前地图 / 上一张地图记录正确

### 5.3 传送门切换

验证：

- 指定目标地图传送正常
- 指定目标 `PointId` 时落点正确
- 接入连接表后不会和普通切图逻辑冲突

### 5.4 存档读档

验证：

- 存档后重进游戏仍能回到正确地图
- `SaveMapId / CurrentMapId / PreviousMapId` 正常工作
- 不再依赖任何 node guid

### 5.5 特殊地图逻辑

验证以下行为不回归：

- `CaveMap` 的蜘蛛 Boss 败北后逻辑
- `MirrorMap1` 的音乐/UI/镜像状态
- `MirrorMap2` 的形态切换逻辑
- 从 `MirrorMap2` 回 `CaveMap` 的后续流程

---

## 六、风险与注意事项

### 6.1 最大风险

不是建 `SO`，而是**过渡期间出现两套地图身份并存**。

典型脏状态：

- 一部分逻辑读 `sceneName`
- 一部分逻辑读 `MapId`
- 一部分逻辑还读 `nodeGuid`

这个状态最恶心，必须避免。

### 6.2 迁移原则

必须遵守：

1. 先定义最终结构
2. 再分阶段迁移
3. 每个阶段都保持可运行
4. 运行时链路切干净后再删旧代码

### 6.3 不做的事

这次明确不做：

- 旧档兼容
- 地图编辑器保留共存
- 在 `MapDefinitionSO` 上继续维护方向字段
- 在场景点位上额外配置“出口/入口”身份字段
- 顺手重构别的系统

别贪。先把这块做干净。

---

## 七、建议执行顺序

建议直接按下面顺序做：

1. 新建 `MapDefinitionSO`
2. 配好 6 张地图资源
3. 新建 `MapConnectionDatabaseSO`
4. 配完所有连接的 `ConnectionId / EndA / EndB`
5. 改 `GameSaveData`
6. 改 `GameManager`
7. 改场景切图点组件和 `MapTeleport`
8. 改场景和默认配置资源引用
9. 验证跑通
10. 删除 `MapGraph` 旧代码和旧资产

---

## 八、结论

这次重构本质上不是“换个数据类”，而是：

**把地图系统从图编辑器驱动，改成连接表驱动的显式配置系统。**

对这个项目当前规模来说，这么改是对的。

原因很简单：

- 结构更清晰
- 数据源更统一
- 场景切换不再藏在 port 和 edge 里
- 存档更干净
- 后续维护成本更低

现在这套 `MapGraph` 留着，收益远小于成本。删掉没毛病。
