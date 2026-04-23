using System;
using System.Collections.Generic;
using UnityEngine;

public class MapLinkPointManager : BaseManager<MapLinkPointManager>
{
    private readonly Dictionary<string, MapLinkPoint> _mapLinkPoints = new Dictionary<string, MapLinkPoint>();

    private MapLinkPointManager() { }

    public void Register(MapLinkPoint point)
    {
        if (point == null || string.IsNullOrWhiteSpace(point.PointId))
        {
            Debug.LogError("MapLinkPoint PointId 不能为空");
            return;
        }

        if (_mapLinkPoints.TryGetValue(point.PointId, out MapLinkPoint existingPoint) && existingPoint != point)
        {
            Debug.LogError($"重复的 MapLinkPoint PointId: {point.PointId}");
            return;
        }

        _mapLinkPoints[point.PointId] = point;
    }

    public void Unregister(MapLinkPoint point)
    {
        if (point == null || string.IsNullOrWhiteSpace(point.PointId))
        {
            return;
        }

        if (_mapLinkPoints.TryGetValue(point.PointId, out MapLinkPoint existingPoint) && existingPoint == point)
        {
            _mapLinkPoints.Remove(point.PointId);
        }
    }
    public void Clear()
    {
        _mapLinkPoints.Clear();
    }
    public Vector3 GetPointPositionOrThrow(string pointId)
    {
        if (_mapLinkPoints.TryGetValue(pointId, out MapLinkPoint point))
        {
            return point.transform.position;
        }

        throw new InvalidOperationException($"未找到 MapLinkPoint: {pointId}");
    }
}
