# MapSystem SO 重构规划

**制定日期**: 2026-04-23
**目标**: 移除 `MapGraph` 图编辑器依赖，将关卡存储和切换统一收敛为 ScriptableObject 配置。

**执行分工**:

- 代码改动由 AI 负责
- Unity 资源创建、场景组件挂载、Inspector 引用配置、旧资源清理由用户手动完成

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
- `PointId` 只要求在**同一张地图内唯一**
- 连接查找使用 `MapId + PointId` 组合定位端点

### 2.3 场景点位组件

普通地图切换不再使用“方向型出生点”。

改为拆成两个组件：

#### `MapLinkPoint`

- `ConnectionId`
  - 当前点属于哪条连接
- `PointId`
  - 当前点在本地图中的稳定标识

说明：

- 这是数据组件
- 既可以挂在“可触发出口点”上，也可以挂在“纯落地点”上
- 不再单独存“我是出口还是入口”，因为这不是稳定数据

#### `MapTransitionTrigger`

- `LoadingSprite`
- `LoadingTime`

说明：

- 这是行为组件
- 只负责触发切图
- 只有需要让玩家碰撞触发切图的点才挂它
- `MapExit` 直接废弃，但过渡阶段暂时保留，不再继续扩展
- 新系统统一新写 `MapTransitionTrigger`

最终效果：

- 纯落地点：只有 `MapLinkPoint`
- 可触发切图点：`MapLinkPoint + MapTransitionTrigger`

### 2.4 特殊点位

现有出生点体系不全部删除。

保留：

- 存档点
- 传送点

删除其在“普通地图切换”中的职责：

- 不再用 `EastPoint / WestPoint / NorthPoint / SouthPoint`
- 不再用 `GetSpawnPositionFromSpawnType(E_SpawnType)` 驱动普通切图

说明：

- 存档点和传送点仍然可以继续按 `ID` 查位置
- 普通地图切换改为直接按连接表对端的 `PointId` 找目标落点
- 本轮先继续沿用现有 `SpawnPoint / SpawnPointManager`
- 不顺手重构存档点和传送点

### 2.5 GameManager 持有的数据

`GameManager` 改为持有：

- `InitialMap`
  - 新游戏默认地图
- `MapDefinitions`
  - 全量地图配置列表
- `ConnectionDatabase`
  - 全量地图连接数据

运行时通过 `MapId` 或 `SceneName` 在该列表中查找目标地图，不再依赖 `MapGraph`

### 2.6 存档结构

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

1. 玩家进入某个同时挂有 `MapLinkPoint + MapTransitionTrigger` 的切图点
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
- 普通切图不再依赖 `SpawnType`
- 纯落地点不需要挂触发器
- 不做 fallback
- 配置缺失、连接找不到、目标点不存在时直接报错

### 3.3 传送门切图

当前 `MapTeleport` 使用 `E_MapSceneName`。

重构后改为：

- 传送门继续保留为特殊逻辑
- 它不强制并入普通连接表链路
- 但最终仍然必须落到：
  - 目标地图 `MapId`
  - 目标落点 `PointId`

### 3.4 存档点与传送点

这两类点先保留现有体系，作为特殊点位继续使用。

保留原因：

- 存档点本来就不是普通地图连接点
- 传送点本来就不是普通双向连接关系
- 先复用现有按 `ID` 查位置的思路，风险最低

本次重构只明确拆掉一件事：

- 普通地图切换不再走 `SpawnType`

好处：

- 不再需要 `GetSceneNameFromEnum`
- 不再多维护一套地图枚举映射

### 3.5 特殊地图逻辑

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

1. 新建 `MapDefinitionSO` 代码类型
2. 由用户手动为现有地图分别创建资源
   - `CampMap`
   - `MagicMap`
   - `CaveMap`
   - `FoxMap`
   - `MirrorMap1`
   - `MirrorMap2`
3. 新建 `MapConnectionDatabaseSO` 代码类型
4. 由用户手动配置所有地图连接数据
   - 每条连接有唯一 `ConnectionId`
   - 每端记录 `MapId + PointId`
5. 设计普通连接点组件：
   - `MapLinkPoint`
   - `MapTransitionTrigger`
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
8. 删除普通切图对 `GetSpawnPositionFromSpawnType(E_SpawnType)` 的依赖

完成标准：

- `GameManager` 运行时不再调用任何 `MapGraph.GetNodeByID / GetEdgeByPortID / GetNodes`
- `GameManager` 运行时不再依赖方向字段推导目标地图
- 普通地图切换不再依赖 `SpawnType`

### Phase 3：切场景与配置引用

目标：让场景和配置资源彻底引用新数据。

工作项：

1. AI 提供场景点位组件代码：
   - `MapLinkPoint`
   - `MapTransitionTrigger`
2. 由用户手动让每个普通切图点配置：
   - `ConnectionId`
   - `PointId`
3. 由用户手动让纯落地点只保留 `MapLinkPoint`
4. `MapTeleport` 从 `E_MapSceneName` 改成新体系
5. 由用户手动把 `SelectMap` 场景中的 `GameManager` 序列化数据切到：
   - `InitialMap`
   - `MapDefinitions`
   - `ConnectionDatabase`
6. 由用户手动把 `GameConfig_main.asset` 默认存档改成新字段

### Phase 3.5：收缩旧出生点职责

目标：只保留旧出生点体系中真正特殊的部分。

工作项：

1. 保留 `SavePoint`
2. 保留 `TeleportPoint`
3. 删除普通地图切换对方向型出生点的依赖
4. 视代码落地情况，移除或废弃：
   - `EastPoint`
   - `WestPoint`
   - `NorthPoint`
   - `SouthPoint`
   - Hidden 版本
5. 删除 `GetSpawnPositionFromSpawnType(E_SpawnType)` 或让其仅服务遗留特殊逻辑

### Phase 4：清理旧系统

目标：在新系统由用户手动验证跑通后，再决定是否删除旧系统。

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
- **先由用户手动跑通测试**
- **再询问用户是否删除旧结构**
- 别反过来，反过来就是纯找死

执行说明：

- 代码文件删除和代码引用清理由 AI 负责
- Unity 资源删除、场景中丢失引用修正、Inspector 清理由用户手动完成
- 在用户没有明确确认前，不执行旧系统删除

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
- 纯落地点没有触发器也能正常作为目标点使用

### 5.3 传送门切换

验证：

- 指定目标地图传送正常
- 指定目标 `PointId` 时落点正确
- 接入连接表后不会和普通切图逻辑冲突
- 传送点仍然可以继续作为特殊点位独立工作

### 5.4 存档点

验证：

- 读档仍然能按存档点位置恢复
- 存档点逻辑不受普通切图重构影响

### 5.5 存档读档

验证：

- 存档后重进游戏仍能回到正确地图
- `SaveMapId / CurrentMapId / PreviousMapId` 正常工作
- 不再依赖任何 node guid

### 5.6 特殊地图逻辑

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
5. 不为新旧链路长期共存设计 fallback

### 6.3 已锁定实现决策

1. `MapExit` 直接废弃，但过渡期暂时保留；新系统新增 `MapTransitionTrigger`
2. `PointId` 只要求同一张地图内唯一
3. 存档点和传送点本轮继续沿用 `SpawnPoint / SpawnPointManager`
4. 新系统不做 fallback，配置缺失时直接报错

### 6.4 不做的事

这次明确不做：

- 旧档兼容
- 地图编辑器保留共存
- 在 `MapDefinitionSO` 上继续维护方向字段
- 在场景点位上额外配置“出口/入口”身份字段
- 继续让普通地图切换依赖 `SpawnType`
- 顺手重构别的系统

别贪。先把这块做干净。

---

## 七、建议执行顺序

建议直接按下面顺序做：

1. 新建 `MapDefinitionSO`
2. 由用户手动配好 6 张地图资源
3. 新建 `MapConnectionDatabaseSO`
4. 由用户手动配完所有连接的 `ConnectionId / EndA / EndB`
5. 改 `GameSaveData`
6. 改 `GameManager`
7. 改普通切图点为 `MapLinkPoint + MapTransitionTrigger`
8. 保留存档点 / 传送点特殊体系
9. 改 `MapTeleport`
10. 由用户手动改场景和默认配置资源引用
11. 验证跑通
12. 由用户手动验证新系统跑通
13. 询问用户是否删除 `MapGraph` 旧系统
14. 用户确认后，再删除 `MapGraph` 旧代码，旧资产由用户手动清理

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
