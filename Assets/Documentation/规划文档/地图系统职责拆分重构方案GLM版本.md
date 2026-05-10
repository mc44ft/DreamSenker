# 地图系统职责拆分重构方案

## 背景

`GameManager` 同时承担地图查询、场景加载、玩家传送、特殊地图逻辑、相机参数同步、存档字段更新，改动热点集中，新增地图配置必改 GameManager。本次只拆地图系统，其余职责不动。

## 阶段定义

**第一阶段（本方案执行范围）：** 把地图系统从 GameManager 中拆出。GameManager 保留外部入口做薄转发。

**第二阶段（不执行）：** 继续拆 Boss 流程、对话 UI 入口、存档/初始化流程；评估是否引入运行时状态机。

## 目标架构

```text
GameManager
  -> MapFlowController
      -> MapRuntimeQuery
      -> SceneTransition
      -> SpawnPointManager
      -> MapLinkPointManager
      -> CameraManager
      -> SpecialMapCoordinator

CameraManager -> GameManager facade -> MapFlowController -> MapRuntimeQuery

SpecialMapCoordinator
  -> GameSaveData / PlayerController / PlayerMirrorEffect
  -> UIManager / AudioManager / EventCenter
```

| 类 | 职责 |
|---|---|
| **GameManager** | 持有 Player、GameSaveData；保留 ChangeMap/TeleportMap/GetMapIdFromEnum/CheckGameCondition/TryGetCurrentMap* 作为 facade 薄转发到 MapFlowController；保留 OnGameBossDead、OnDialogueNodeShowPanel、LoadGame、InitializeGame、SaveDataAll |
| **MapRuntimeQuery** | 持有 MapConnectionDatabaseSO；提供 MapDefinitionSO 查询、连接对端查询、相机参数查询。由 MapFlowController 内部持有，不单独暴露给 GameManager |
| **MapFlowController** | 负责普通换图和传送；切图前清理点位缓存；切图后移动玩家、刷新相机、更新存档 CurrentMapId/PreviousMapId；内部持有 MapRuntimeQuery 并对外封装查询方法 |
| **SpecialMapCoordinator** | 负责 CaveMap/MirrorMap1/MirrorMap2 进入后的表现和状态修正（音乐、UI、镜像、玩家形态、特殊存档字段） |

## 约束

- **生命周期**：MapFlowController、SpecialMapCoordinator 为非单例纯 C# 类，由 GameManager 在 InitializeGame() 中 new 并持有引用，依赖通过构造函数注入。MapRuntimeQuery 由 MapFlowController 内部 new 并持有，不单独暴露给 GameManager。InitializeGame() 在场景加载完成、所有 MonoBehaviour Awake() 执行后才调用，依赖均已就绪。
- **facade 保留**：GameManager 第一阶段保留 ChangeMap/TeleportMap/TryGetCurrentMap* 等公开方法做薄转发（查询方法转发到 MapFlowController 对 MapRuntimeQuery 的封装），降低调用面风险。第二阶段再评估是否移除。
- **异步控制**：暂不处理加载期间重复触发 ChangeMap 等并发问题，等实际遇到再补。
- **SpecialMapCoordinator 扩展**：只做搬家，保持现有 switch 分支，新增特殊地图继续加分支，后续再考虑策略模式或查表。
- **不修改资产文件**：不安排修改 Prefab、Scene、ScriptableObject、.asset、.unity、.meta。新增脚本导致的 .meta 由 Unity 自动生成，只报告不手动处理。
- **不删除/移动现有脚本**：除非执行前单独确认 .cs.meta 怎么处理。

## 行为不变项

- 外部仍调用 GameManager.Instance.ChangeMap(pointGuid)
- 外部仍调用 GameManager.Instance.TeleportMap(mapId, teleportID)
- MapTransitionTrigger 仍通过 MapLinkPoint.PointGuid 触发换图
- MapConnectionDatabaseSO 仍是连接数据来源
- MapRegistrySO 仍是地图定义清单来源
- MapDefinitionSO 继续保存地图 ID、场景名、玩家阴影、相机参数等配置

## 执行步骤

### 1. 新建 MapRuntimeQuery，迁移纯查询逻辑

新增 `Assets/Scripts/MapSystem/MapRuntimeQuery.cs`，构造时接收 MapConnectionDatabaseSO。由 MapFlowController 内部 new 并持有。

提供方法：
- GetRequiredMapById / GetRequiredMapBySceneName
- TryFindMapBySceneName / TryFindCurrentMap
- GetOtherEndpointOrThrow
- TryGetCurrentMapFollowCameraOrthoSize / TryGetCurrentMapBossCameraOrthoSize / TryGetCurrentMapDialogueCameraSettings

验证：GameManager 内部查询方法转发到 MapRuntimeQuery 后，地图参数、地图 ID、连接查询结果不变。

### 2. 新建 SpecialMapCoordinator，迁移 SpecialMapInitialize 逻辑

新增 `Assets/Scripts/MapSystem/SpecialMapCoordinator.cs`，构造时接收 GameSaveData、PlayerController、PlayerMirrorEffect。地图查询通过 MapFlowController 获取。

GameManager.SpecialMapInitialize() 改为转发到 SpecialMapCoordinator.InitializeCurrentMap()。

验证：CaveMap、MirrorMap1、MirrorMap2 行为不变。

### 3. 新建 MapFlowController，迁移切图流程

新增 `Assets/Scripts/MapSystem/MapFlowController.cs`，构造时接收 SceneTransition、SpawnPointManager、MapLinkPointManager、CameraManager、SpecialMapCoordinator。内部 new MapRuntimeQuery 并持有。

迁移方法：ChangeMap、TeleportMap、LoadMap、LoadMapScene、ChangePlayerPosition。

GameManager.ChangeMap() 和 GameManager.TeleportMap() 改为薄转发。

验证：MapTransitionTrigger 不需要修改，换图流程行为不变。

### 4. CameraManager 地图参数来源切换

CameraManager 的地图相机参数读取从 GameManager facade 转到 MapFlowController 对 MapRuntimeQuery 的封装方法。

验证：Boss 镜头、对话镜头参数获取不变。

### 5. 清理确认

用 rg 检查 GameManager 中剩余的 MapDefinitionSO、MapConnectionDatabaseSO、MapLinkPointManager、SpawnPointManager 直接引用，确认地图职责已移出。

验证：Unity Console 无 Error。

## 关键标识符

| 标识符 | 说明 |
|---|---|
| MapDefinitionSO | 地图配置 SO，Assets/Scripts/MapSystem/Data/MapDefinitionSO.cs |
| MapConnectionDatabaseSO | 地图连接数据库 SO，Assets/Scripts/MapSystem/Data/MapConnectionDatabaseSO.cs |
| MapRegistrySO | 地图配置清单 SO，Assets/Scripts/MapSystem/Data/MapRegistrySO.cs |
| MapEndpointData | 地图连接端点数据结构，定义在 MapConnectionDatabaseSO.cs |
| MapLinkPoint.PointGuid | 地图连接点唯一 ID，Assets/Scripts/MapSystem/MapLinkPoint.cs |
| EMapSceneName | 地图场景枚举，当前由 GameManager.GetSceneNameFromEnum() 使用 |
