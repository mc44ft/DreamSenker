# 地图系统职责拆分重构方案 GPT版本

## 计划说明

这份计划要解决一个核心问题：`GameManager` 现在把地图查询、地图切换、玩家落点、特殊地图逻辑、相机参数转发都抓在自己手里，导致每次改地图功能都要继续往 `GameManager` 塞代码。

执行后会得到三个直接效果：

- 地图相关代码集中到 `MapFlowController`、`MapRuntimeQuery`、`SpecialMapCoordinator`，以后改地图流程不用一直进 `GameManager`。
- `GameManager` 仍保留外部入口，旧调用不需要大面积改，风险可控。
- 后续再拆 Boss、对话、存档时，`GameManager` 已经少掉最大的一块职责，第二阶段会更容易。

一句话：本计划不是为了“拆文件好看”，而是先把最近最常改、最容易继续膨胀的地图系统从 `GameManager` 里切出去。

## 结论

本方案只做第一阶段：把地图系统从 `GameManager` 中拆出来。

第一阶段只拆：

- 地图查询
- 地图切换
- 玩家切图落点
- 特殊地图进入逻辑

第一阶段不拆：

- Boss 死亡流程
- 对话外部 UI 入口
- 存档和游戏初始化
- UI / Audio / EventCenter
- `CameraManager` 的整体架构

第二阶段再拆 Boss、对话、存档、游戏状态机。

## 最终结构

```text
GameManager
  -> MapFlowController

MapFlowController
  -> MapRuntimeQuery
  -> SpecialMapCoordinator
  -> SceneTransition
  -> SpawnPointManager
  -> MapLinkPointManager
  -> CameraManager

CameraManager
  -> GameManager facade
      -> MapFlowController
          -> MapRuntimeQuery
```

第一阶段 `GameManager` 继续当 facade。  
`CameraManager` 不直接依赖 `MapRuntimeQuery`。

## 改动清单

### 新增 `Assets/Scripts/MapSystem/MapRuntimeQuery.cs`

职责：

- 查询 `MapDefinitionSO`。
- 查询当前地图。
- 查询地图连接对端。
- 查询当前地图的相机参数。

来源：

- 由 `MapFlowController` 持有。
- `MapFlowController` 创建它时注入 `MapConnectionDatabaseSO`。
- 普通 C# 类，不做 `MonoBehaviour`，不做单例。

迁移内容：

- `GetRequiredMapById`
- `GetRequiredMapBySceneName`
- `FindMapById`
- `FindMapBySceneName`
- `TryFindCurrentMap`
- `TryFindMapBySceneName`
- `GetOtherEndpointOrThrow`
- `TryGetCurrentMapFollowCameraOrthoSize`
- `TryGetCurrentMapBossCameraOrthoSize`
- `TryGetCurrentMapDialogueCameraSettings`

### 新增 `Assets/Scripts/MapSystem/MapFlowController.cs`

职责：

- 普通连接换图。
- 指定地图传送。
- 持有并使用 `MapRuntimeQuery`。
- 持有并使用 `SpecialMapCoordinator`。
- 切图前清理点位缓存。
- 切图后移动玩家。
- 切图后刷新相机。
- 更新 `GameSaveData.CurrentMapId` 和 `GameSaveData.PreviousMapId`。

迁移内容：

- `ChangeMap`
- `TeleportMap`
- `LoadMap`
- `LoadMapScene`
- `ChangePlayerPosition`

### 新增 `Assets/Scripts/MapSystem/SpecialMapCoordinator.cs`

职责：

- 只处理特殊地图进入后的表现和状态修正。

包含：

- CaveMap 蜘蛛死亡后的 `Game_BossKeepDead`。
- 从 MirrorMap2 回到 CaveMap 后的奖励逻辑。
- MirrorMap1 的音乐、UI、镜像表现。
- MirrorMap2 的形态切换逻辑。

不包含：

- 场景加载。
- 玩家生成。
- 普通地图切换。
- 地图连接查询。
- 相机参数查询。

### 修改 `Assets/Scripts/Managers/GameManager.cs`

保留：

- `Player`
- `GameSaveData`
- `LoadGame`
- `InitializeGame`
- `SaveDataAll`
- `OnGameBossDead`
- `OnDialogueNodeShowPanel`
- `CheckGameCondition`

改成转发：

- `ChangeMap`
- `TeleportMap`
- `GetMapIdFromEnum`
- `TryGetCurrentMapFollowCameraOrthoSize`
- `TryGetCurrentMapBossCameraOrthoSize`
- `TryGetCurrentMapDialogueCameraSettings`

新增字段：

```csharp
private MapFlowController _mapFlowController;
```

初始化：

```csharp
_mapFlowController = new MapFlowController(ConnectionDatabase, ...);
```

### 修改 `Assets/Scripts/CameraSystem/CameraManager.cs`

第一阶段不改依赖方向。

保持：

```csharp
GameManager.Instance.TryGetCurrentMapBossCameraOrthoSize(...)
GameManager.Instance.TryGetCurrentMapDialogueCameraSettings(...)
```

这些方法由 `GameManager` 转发给 `MapFlowController`，再由 `MapFlowController` 使用 `MapRuntimeQuery` 查询。

### 不修改 `Assets/Scripts/MapSystem/MapTransitionTrigger.cs`

保持：

```csharp
GameManager.Instance.ChangeMap(_mapLinkPoint.PointGuid);
```

因为 `GameManager.ChangeMap()` 会变成薄转发。

## 执行顺序

1. 新建 `MapRuntimeQuery`，先迁移纯查询逻辑。
2. 新建 `MapFlowController`，由它持有并使用 `MapRuntimeQuery`。
3. 新建 `SpecialMapCoordinator`，迁移 `SpecialMapInitialize()`。
4. 让 `MapFlowController` 持有并调用 `SpecialMapCoordinator`。
5. 将切图流程迁移进 `MapFlowController`。
6. 让 `GameManager.ChangeMap()` 和 `TeleportMap()` 变成薄转发。
7. 让 `GameManager` 的地图查询 facade 转发到 `MapFlowController`。
8. 保持 `CameraManager` 继续通过 `GameManager` facade 读取地图相机参数。
9. 用 `rg` 检查 `GameManager` 剩余地图职责。

## 验证

代码检查：

- `GameManager.cs` 不再实现地图查询细节。
- `GameManager.cs` 不再直接编排完整切图流程。
- `GameManager.cs` 不再直接包含 `SpecialMapInitialize()` 具体逻辑。
- `GameManager.cs` 不直接持有 `MapRuntimeQuery`。
- `MapFlowController.cs` 持有并使用 `MapRuntimeQuery`。
- `CameraManager.cs` 仍通过 `GameManager` facade 读取地图相机参数。
- `SpecialMapCoordinator.cs` 不出现 `SceneTransition`、`ChangePlayerPosition`、`GetOtherEndpointOrThrow`。

功能验证：

- 普通地图连接能往返。
- `TeleportMap(mapId, teleportID)` 能到指定出生点。
- `TeleportMap(mapId)` 能到 `ESpawnType.TeleportPoint`。
- 切图后相机不再偏移。
- 进入地图后 Follow VCam 使用 `FollowCameraOrthoSize`。
- 进入 Boss 战后 Follow VCam 使用 `FollowCameraBossOrthoSize`。
- 退出 Boss 战后 Follow VCam 恢复 `FollowCameraOrthoSize`。
- 进入对话后 Dialogue VCam 使用 `DialogueCameraOrthoSize` 和 `DialogueCameraOffsetY`。
- MirrorMap1 / MirrorMap2 / CaveMap 特殊逻辑保持不变。

基础验证：

- `rg` 检查关键引用。
- `git diff --check` 通过。
- Unity Console 最近无 Error。

## 禁止项

- 不修改 prefab。
- 不修改 scene。
- 不修改 asset。
- 不修改 meta。
- 不删除或移动脚本。
- 不拆 Boss、对话、存档、UI。
- 不引入完整 DI 框架。
- 不引入全局状态机。
