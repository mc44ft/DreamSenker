using System;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Characters.Player;
using DreamSeeker.Data.Runtime;
using DreamSeeker.MapSystem.Data;
using DreamSeeker.MapSystem.SpawnPoints;
using DreamSeeker.Shared;

namespace DreamSeeker.MapSystem
{
/// <summary>
/// 地图流程控制器：负责换图、传送、玩家落点和特殊地图初始化编排。
/// </summary>
public class MapFlowController
{
    private readonly GameSaveData _gameSaveData;//当前游戏存档数据
    private readonly Func<PlayerController> _playerGetter;//玩家控制器获取入口
    private readonly Func<PlayerMirrorEffect> _mirrorEffectGetter;//玩家镜像表现组件获取入口
    private readonly MapRuntimeQuery _runtimeQuery;//地图运行时查询器
    private readonly SpecialMapCoordinator _specialMapCoordinator;//特殊地图表现协调器

    /// <summary>
    /// 创建地图流程控制器，并初始化内部地图查询器和特殊地图协调器。
    /// </summary>
    public MapFlowController(
        MapConnectionDatabaseSO connectionDatabase,
        GameSaveData gameSaveData,
        Func<PlayerController> playerGetter,
        Func<PlayerMirrorEffect> mirrorEffectGetter,
        object eventSender)
    {
        _gameSaveData = gameSaveData;
        _playerGetter = playerGetter;
        _mirrorEffectGetter = mirrorEffectGetter;
        _runtimeQuery = new MapRuntimeQuery(connectionDatabase);
        _specialMapCoordinator = new SpecialMapCoordinator(gameSaveData, playerGetter, mirrorEffectGetter, eventSender);
    }

    /// <summary>
    /// 按地图 ID 获取地图配置；供 GameManager 初始化读档流程使用。
    /// </summary>
    public MapDefinitionSO GetRequiredMapById(string mapId)
    {
        return _runtimeQuery.GetRequiredMapById(mapId);
    }

    /// <summary>
    /// 按地图场景枚举获取地图 ID。
    /// </summary>
    public string GetMapIdFromEnum(EMapSceneName sceneName)
    {
        return _runtimeQuery.GetRequiredMapBySceneName(MapRuntimeQuery.GetSceneNameFromEnum(sceneName)).MapId;
    }

    /// <summary>
    /// 将地图场景枚举转换为真实场景名。
    /// </summary>
    public string GetSceneNameFromEnum(EMapSceneName sceneName)
    {
        return MapRuntimeQuery.GetSceneNameFromEnum(sceneName);
    }

    /// <summary>
    /// 按地图配置加载场景。
    /// </summary>
    public void LoadMap(MapDefinitionSO mapDefinition, Action onFinished = null)
    {
        LoadMapScene(mapDefinition.SceneName, onFinished);
    }

    /// <summary>
    /// 普通地图连接换图。
    /// </summary>
    public void ChangeMap(string pointGuid)
    {
        ClearScenePointCaches();

        MapEndpointData targetEndpoint = _runtimeQuery.GetOtherEndpointOrThrow(_gameSaveData.CurrentMapId, pointGuid);
        MapDefinitionSO targetMap = _runtimeQuery.GetRequiredMapById(targetEndpoint.MapId);

        UpdateCurrentMap(targetMap);

        LoadMapScene(targetMap.SceneName, () =>
        {
            Vector3 position = MapLinkPointManager.Instance.GetPointPositionOrThrow(targetEndpoint.PointGuid);
            MovePlayerTo(position);
            ApplyMapVisualState(targetMap);
            InitializeCurrentSpecialMap();
        });
    }

    /// <summary>
    /// 指定地图传送；teleportID 为空时使用 TeleportPoint 类型出生点。
    /// </summary>
    public void TeleportMap(string mapId, string teleportID = "")
    {
        ClearScenePointCaches();

        MapDefinitionSO targetMap = _runtimeQuery.GetRequiredMapById(mapId);

        UpdateCurrentMap(targetMap);

        LoadMapScene(targetMap.SceneName, () =>
        {
            Vector3 position = !string.IsNullOrWhiteSpace(teleportID)
                ? SpawnPointManager.Instance.GetSpawnPositionFromID(teleportID)
                : SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(ESpawnType.TeleportPoint);

            MovePlayerTo(position);
            ApplyMapVisualState(targetMap);
            InitializeCurrentSpecialMap();
        });
    }

    /// <summary>
    /// 初始化当前地图的特殊表现和状态修正。
    /// </summary>
    public void InitializeCurrentSpecialMap()
    {
        MapDefinitionSO currentMap = _runtimeQuery.GetRequiredMapById(_gameSaveData.CurrentMapId);
        MapDefinitionSO previousMap = string.IsNullOrWhiteSpace(_gameSaveData.PreviousMapId)
            ? null
            : _runtimeQuery.GetRequiredMapById(_gameSaveData.PreviousMapId);

        _specialMapCoordinator.InitializeCurrentMap(currentMap, previousMap);
    }

    /// <summary>
    /// 按场景名尝试获取日常跟随相机视野；非地图场景返回 false。
    /// </summary>
    public bool TryGetFollowCameraOrthoSizeBySceneName(string sceneName, out float orthoSize)
    {
        orthoSize = 0f;

        if (!_runtimeQuery.TryFindMapBySceneName(sceneName, out MapDefinitionSO map))
        {
            return false;
        }

        orthoSize = map.FollowCameraOrthoSize;
        return true;
    }

    /// <summary>
    /// 尝试获取当前地图日常跟随相机正交视野大小。
    /// </summary>
    public bool TryGetCurrentMapFollowCameraOrthoSize(out float orthoSize)
    {
        return _runtimeQuery.TryGetCurrentMapFollowCameraOrthoSize(_gameSaveData, out orthoSize);
    }

    /// <summary>
    /// 尝试获取当前地图 Boss 战跟随相机正交视野大小。
    /// </summary>
    public bool TryGetCurrentMapBossCameraOrthoSize(out float orthoSize)
    {
        return _runtimeQuery.TryGetCurrentMapBossCameraOrthoSize(_gameSaveData, out orthoSize);
    }

    /// <summary>
    /// 尝试获取当前地图对话相机正交视野和Y轴偏移配置。
    /// </summary>
    public bool TryGetCurrentMapDialogueCameraSettings(out float orthoSize, out float offsetY)
    {
        return _runtimeQuery.TryGetCurrentMapDialogueCameraSettings(_gameSaveData, out orthoSize, out offsetY);
    }

    /// <summary>
    /// 加载地图场景。
    /// </summary>
    private void LoadMapScene(string mapName, Action onFinished = null)
    {
        SceneTransition.Instance.LoadScene(mapName, null, onFinished);
    }

    /// <summary>
    /// 更新存档中的当前地图和上一张地图。
    /// </summary>
    private void UpdateCurrentMap(MapDefinitionSO targetMap)
    {
        _gameSaveData.PreviousMapId = _gameSaveData.CurrentMapId;
        _gameSaveData.CurrentMapId = targetMap.MapId;
    }

    /// <summary>
    /// 切图前清空当前场景点位缓存。
    /// </summary>
    private void ClearScenePointCaches()
    {
        SpawnPointManager.Instance.ClearSpawnPointDict();
        MapLinkPointManager.Instance.Clear();
    }

    /// <summary>
    /// 移动玩家并通知 Cinemachine 目标发生瞬移。
    /// </summary>
    private void MovePlayerTo(Vector3 position)
    {
        PlayerController player = GetPlayerOrThrow();
        Vector3 previousPosition = player.transform.position;
        player.transform.position = position;
        CameraManager.Instance?.RefreshFollowCameraAfterPlayerWarp(previousPosition, position);
    }

    /// <summary>
    /// 应用地图进入后的玩家阴影表现。
    /// </summary>
    private void ApplyMapVisualState(MapDefinitionSO targetMap)
    {
        _mirrorEffectGetter?.Invoke()?.SetShadowDarknessStrength(targetMap.PlayerShadowDarknessStrength);
    }

    /// <summary>
    /// 获取玩家控制器；切图落点需要玩家时，缺失直接暴露错误。
    /// </summary>
    private PlayerController GetPlayerOrThrow()
    {
        PlayerController player = _playerGetter?.Invoke();
        if (player == null)
        {
            throw new InvalidOperationException("地图切换需要 PlayerController，但当前玩家为空");
        }

        return player;
    }
}
}
