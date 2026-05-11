using System;
using System.Linq;

using DreamSenker.Data.Runtime;
using DreamSenker.MapSystem.Data;
using DreamSenker.Shared;

namespace DreamSenker.MapSystem
{
/// <summary>
/// 地图运行时查询器：只负责从地图配置和连接数据库中读取数据。
/// </summary>
public class MapRuntimeQuery
{
    private readonly MapConnectionDatabaseSO _connectionDatabase;//地图连接数据库配置

    /// <summary>
    /// 创建地图运行时查询器。
    /// </summary>
    public MapRuntimeQuery(MapConnectionDatabaseSO connectionDatabase)
    {
        _connectionDatabase = connectionDatabase;
    }

    /// <summary>
    /// 按地图 ID 获取地图配置；找不到时抛出明确错误。
    /// </summary>
    public MapDefinitionSO GetRequiredMapById(string mapId)
    {
        MapDefinitionSO map = FindMapById(mapId);
        if (map == null)
        {
            throw new InvalidOperationException($"未找到 MapDefinitionSO: {mapId}");
        }

        return map;
    }

    /// <summary>
    /// 按场景名获取地图配置；找不到时抛出明确错误。
    /// </summary>
    public MapDefinitionSO GetRequiredMapBySceneName(string sceneName)
    {
        MapDefinitionSO map = FindMapBySceneName(sceneName);
        if (map == null)
        {
            throw new InvalidOperationException($"未找到场景对应的 MapDefinitionSO: {sceneName}");
        }

        return map;
    }

    /// <summary>
    /// 按地图 ID 查找地图配置；找不到时返回 null。
    /// </summary>
    public MapDefinitionSO FindMapById(string mapId)
    {
        return GetMapRegistryOrThrow().AllMaps?.FirstOrDefault(map => map != null && map.MapId == mapId);
    }

    /// <summary>
    /// 按场景名查找地图配置；找不到时返回 null。
    /// </summary>
    public MapDefinitionSO FindMapBySceneName(string sceneName)
    {
        return GetMapRegistryOrThrow().AllMaps?.FirstOrDefault(map => map != null && map.SceneName == sceneName);
    }

    /// <summary>
    /// 尝试获取当前存档指向的地图配置。
    /// </summary>
    public bool TryFindCurrentMap(GameSaveData gameSaveData, out MapDefinitionSO map)
    {
        map = null;

        if (gameSaveData == null || string.IsNullOrWhiteSpace(gameSaveData.CurrentMapId) || _connectionDatabase == null || _connectionDatabase.MapRegistry == null)
        {
            return false;
        }

        map = _connectionDatabase.MapRegistry.AllMaps?.FirstOrDefault(definition => definition != null && definition.MapId == gameSaveData.CurrentMapId);
        return map != null;
    }

    /// <summary>
    /// 按场景名尝试查找地图配置；非地图场景返回 false，不抛异常。
    /// </summary>
    public bool TryFindMapBySceneName(string sceneName, out MapDefinitionSO map)
    {
        map = null;

        if (string.IsNullOrWhiteSpace(sceneName) || _connectionDatabase == null || _connectionDatabase.MapRegistry == null)
        {
            return false;
        }

        map = _connectionDatabase.MapRegistry.AllMaps?.FirstOrDefault(definition => definition != null && definition.SceneName == sceneName);
        return map != null;
    }

    /// <summary>
    /// 获取普通地图连接的另一端；连接缺失或不唯一时抛出错误。
    /// </summary>
    public MapEndpointData GetOtherEndpointOrThrow(string currentMapId, string pointGuid)
    {
        if (_connectionDatabase == null)
        {
            throw new InvalidOperationException("ConnectionDatabase 未配置");
        }

        //普通连接必须唯一，0 条和多条都不能继续切图。
        if (!_connectionDatabase.TryGetOtherEndpoint(currentMapId, pointGuid, out MapEndpointData targetEndpoint))
        {
            throw new InvalidOperationException(
                $"未找到唯一连接对端，CurrentMapId={currentMapId}, PointGuid={pointGuid}");
        }

        return targetEndpoint;
    }

    /// <summary>
    /// 尝试获取当前地图日常跟随相机正交视野大小。
    /// </summary>
    public bool TryGetCurrentMapFollowCameraOrthoSize(GameSaveData gameSaveData, out float orthoSize)
    {
        orthoSize = 0f;

        if (!TryFindCurrentMap(gameSaveData, out MapDefinitionSO map))
        {
            return false;
        }

        orthoSize = map.FollowCameraOrthoSize;
        return true;
    }

    /// <summary>
    /// 尝试获取当前地图 Boss 战跟随相机正交视野大小。
    /// </summary>
    public bool TryGetCurrentMapBossCameraOrthoSize(GameSaveData gameSaveData, out float orthoSize)
    {
        orthoSize = 0f;

        if (!TryFindCurrentMap(gameSaveData, out MapDefinitionSO map))
        {
            return false;
        }

        orthoSize = map.FollowCameraBossOrthoSize;
        return true;
    }

    /// <summary>
    /// 尝试获取当前地图对话相机正交视野和Y轴偏移配置。
    /// </summary>
    public bool TryGetCurrentMapDialogueCameraSettings(GameSaveData gameSaveData, out float orthoSize, out float offsetY)
    {
        orthoSize = 0f;
        offsetY = 0f;

        if (!TryFindCurrentMap(gameSaveData, out MapDefinitionSO map))
        {
            return false;
        }

        orthoSize = map.DialogueCameraOrthoSize;
        offsetY = map.DialogueCameraOffsetY;
        return true;
    }

    /// <summary>
    /// 将地图场景枚举转换为真实场景名。
    /// </summary>
    public static string GetSceneNameFromEnum(EMapSceneName sceneName)
    {
        switch (sceneName)
        {
            case EMapSceneName.CampMap:
                return "CampMap";
            case EMapSceneName.MagicMap:
                return "MagicMap";
            case EMapSceneName.CaveMap:
                return "CaveMap";
            case EMapSceneName.FoxMap:
                return "FoxMap";
            case EMapSceneName.MirrorMap1:
                return "MirrorMap1";
            case EMapSceneName.MirrorMap2:
                return "MirrorMap2";
            default:
                return "CampMap";
        }
    }

    /// <summary>
    /// 获取地图注册表；配置缺失时抛出明确错误。
    /// </summary>
    private MapRegistrySO GetMapRegistryOrThrow()
    {
        if (_connectionDatabase == null)
        {
            throw new InvalidOperationException("ConnectionDatabase 未配置");
        }

        if (_connectionDatabase.MapRegistry == null)
        {
            throw new InvalidOperationException("ConnectionDatabase.MapRegistry 未配置");
        }

        return _connectionDatabase.MapRegistry;
    }
}
}
