using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.MapSystem.Data
{
[Serializable]
public struct MapEndpointData
{
    public string MapId;
    public string PointGuid;

    public bool Match(string mapId, string pointGuid)
    {
        return MapId == mapId && PointGuid == pointGuid;
    }
}

[Serializable]
public struct MapConnectionData
{
    public string ConnectionDisplayName;
    public MapEndpointData EndA;
    public MapEndpointData EndB;
}
//缓存扫描地图所有点位的数据结构
[Serializable]
public class MapPointScanCache
{
    public string MapId;
    public List<MapPointScanData> Points = new List<MapPointScanData>();
}

[Serializable]
public class MapPointScanData
{
    public string PointGuid;
    public string DisplayName;
}

[CreateAssetMenu(fileName = "MapConnectionDatabase_", menuName = "ScriptableObject/Map/MapConnectionDatabase")]
public class MapConnectionDatabaseSO : ScriptableObject
{
    [field: SerializeField] public MapRegistrySO MapRegistry { get; private set; }
    //存储所有连接
    [field: SerializeField] public List<MapConnectionData> Connections { get; private set; } = new List<MapConnectionData>();
    //编辑器扫描缓存，用于连接表下拉选择点位
    [field: SerializeField] public List<MapPointScanCache> PointScanCaches { get; private set; } = new List<MapPointScanCache>();

    //获取连接另一端的点位数据
    public bool TryGetOtherEndpoint(string currentMapId, string pointGuid, out MapEndpointData otherEndpoint)
    {
        int matchCount = 0;
        otherEndpoint = default;

        for (int i = 0; i < Connections.Count; i++)
        {
            MapConnectionData connection = Connections[i];

            if (connection.EndA.Match(currentMapId, pointGuid))
            {
                otherEndpoint = connection.EndB;
                matchCount++;
            }

            if (connection.EndB.Match(currentMapId, pointGuid))
            {
                otherEndpoint = connection.EndA;
                matchCount++;
            }
        }

        return matchCount == 1;
    }
}
}
