using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MapEndpointData
{
    public string MapId;
    public string PointId;

    public bool Match(string mapId, string pointId)
    {
        return MapId == mapId && PointId == pointId;
    }
}

[Serializable]
public struct MapConnectionData
{
    public string ConnectionId;
    public MapEndpointData EndA;
    public MapEndpointData EndB;
}

[CreateAssetMenu(fileName = "MapConnectionDatabase_", menuName = "ScriptableObject/Map/MapConnectionDatabase")]
public class MapConnectionDatabaseSO : ScriptableObject
{
    //存储所有连接
    [field: SerializeField] public List<MapConnectionData> Connections { get; private set; } = new List<MapConnectionData>();

    //获取连接另一端的点位数据
    public bool TryGetOtherEndpoint(string currentMapId, string connectionId, string pointId, out MapEndpointData otherEndpoint)
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            MapConnectionData connection = Connections[i];
            if (connection.ConnectionId != connectionId)
            {
                continue;
            }

            if (connection.EndA.Match(currentMapId, pointId))
            {
                otherEndpoint = connection.EndB;
                return true;
            }

            if (connection.EndB.Match(currentMapId, pointId))
            {
                otherEndpoint = connection.EndA;
                return true;
            }
        }

        otherEndpoint = default;
        return false;
    }
}
