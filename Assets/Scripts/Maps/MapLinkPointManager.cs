using System;
using System.Collections.Generic;
using UnityEngine;

public class MapLinkPointManager : BaseManager<MapLinkPointManager>
{
    private readonly Dictionary<string, MapLinkPoint> _mapLinkPoints = new Dictionary<string, MapLinkPoint>();

    private MapLinkPointManager() { }

    public void Register(MapLinkPoint point)
    {
        if (point == null || string.IsNullOrWhiteSpace(point.PointGuid))
        {
            Debug.LogError("MapLinkPoint PointGuid 不能为空");
            return;
        }

        if (_mapLinkPoints.TryGetValue(point.PointGuid, out MapLinkPoint existingPoint) && existingPoint != point)
        {
            Debug.LogError($"重复的 MapLinkPoint PointGuid: {point.PointGuid}");
            return;
        }

        _mapLinkPoints[point.PointGuid] = point;
    }

    public void Unregister(MapLinkPoint point)
    {
        if (point == null || string.IsNullOrWhiteSpace(point.PointGuid))
        {
            return;
        }

        if (_mapLinkPoints.TryGetValue(point.PointGuid, out MapLinkPoint existingPoint) && existingPoint == point)
        {
            _mapLinkPoints.Remove(point.PointGuid);
        }
    }
    public void Clear()
    {
        _mapLinkPoints.Clear();
    }
    public Vector3 GetPointPositionOrThrow(string pointGuid)
    {
        if (_mapLinkPoints.TryGetValue(pointGuid, out MapLinkPoint point))
        {
            return point.transform.position;
        }

        throw new InvalidOperationException($"未找到 MapLinkPoint: {pointGuid}");
    }
}
