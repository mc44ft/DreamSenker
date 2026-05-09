using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamSenker.MapSystem;

namespace DreamSenker.MapSystem.Editor
{
[CustomEditor(typeof(MapConnectionDatabaseSO))]
public class MapConnectionDatabaseSOEditor : UnityEditor.Editor
{
    private SerializedProperty _mapRegistryProperty;
    private SerializedProperty _connectionsProperty;
    private readonly List<string> _validationErrors = new List<string>();
    private readonly List<string> _validationWarnings = new List<string>();
    private bool _hasValidated;

    private void OnEnable()
    {
        _mapRegistryProperty = serializedObject.FindProperty("<MapRegistry>k__BackingField");
        _connectionsProperty = serializedObject.FindProperty("<Connections>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_mapRegistryProperty);
        EditorGUILayout.Space(6);

        DrawToolbar();
        DrawConnections();
        DrawValidationMessages();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawToolbar()
    {
        bool hasRegistry = GetDatabase().MapRegistry != null;

        using (new EditorGUI.DisabledScope(!hasRegistry))
        {
            if (GUILayout.Button("扫描全部点位"))
            {
                ScanAllMapPoints();
            }
        }

        if (!hasRegistry)
        {
            EditorGUILayout.HelpBox("MapRegistrySO 未配置，地图下拉、点位下拉和扫描不可用。", MessageType.Warning);
        }

        if (GUILayout.Button("校验"))
        {
            ValidateDatabase();
        }
    }

    private void DrawConnections()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);

        for (int i = 0; i < _connectionsProperty.arraySize; i++)
        {
            SerializedProperty connection = _connectionsProperty.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"连接 {i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("删除", GUILayout.Width(56)))
            {
                _connectionsProperty.DeleteArrayElementAtIndex(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(connection.FindPropertyRelative("ConnectionDisplayName"), new GUIContent("Display Name"));
            DrawEndpoint(connection.FindPropertyRelative("EndA"), "EndA");
            DrawEndpoint(connection.FindPropertyRelative("EndB"), "EndB");
            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("添加连接"))
        {
            _connectionsProperty.InsertArrayElementAtIndex(_connectionsProperty.arraySize);
            SerializedProperty connection = _connectionsProperty.GetArrayElementAtIndex(_connectionsProperty.arraySize - 1);
            connection.FindPropertyRelative("ConnectionDisplayName").stringValue = string.Empty;
            ClearEndpoint(connection.FindPropertyRelative("EndA"));
            ClearEndpoint(connection.FindPropertyRelative("EndB"));
        }
    }

    private void DrawEndpoint(SerializedProperty endpointProperty, string label)
    {
        SerializedProperty mapIdProperty = endpointProperty.FindPropertyRelative("MapId");
        SerializedProperty pointGuidProperty = endpointProperty.FindPropertyRelative("PointGuid");
        MapRegistrySO registry = GetDatabase().MapRegistry;

        EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);

        if (registry == null || registry.AllMaps == null || registry.AllMaps.Count == 0)
        {
            EditorGUILayout.TextField("MapId", mapIdProperty.stringValue);
            EditorGUILayout.TextField("PointGuid", pointGuidProperty.stringValue);
            return;
        }

        List<MapDefinitionSO> maps = registry.AllMaps.Where(map => map != null).ToList();
        if (maps.Count == 0)
        {
            EditorGUILayout.TextField("MapId", mapIdProperty.stringValue);
            EditorGUILayout.TextField("PointGuid", pointGuidProperty.stringValue);
            EditorGUILayout.HelpBox("MapRegistrySO 没有有效地图。", MessageType.Warning);
            return;
        }

        string[] mapLabels = maps.Select(GetMapLabel).ToArray();
        int currentMapIndex = maps.FindIndex(map => map.MapId == mapIdProperty.stringValue);
        if (currentMapIndex < 0 && !string.IsNullOrWhiteSpace(mapIdProperty.stringValue))
        {
            string[] missingMapLabels = new[] { $"缺失地图 ({mapIdProperty.stringValue})" }.Concat(mapLabels).ToArray();
            int selectedMissingIndex = EditorGUILayout.Popup("Map", 0, missingMapLabels);
            if (selectedMissingIndex == 0)
            {
                EditorGUILayout.TextField("PointGuid", pointGuidProperty.stringValue);
                EditorGUILayout.HelpBox($"当前地图不在 MapRegistrySO 中：{mapIdProperty.stringValue}", MessageType.Error);
                return;
            }

            MapDefinitionSO selectedMap = maps[selectedMissingIndex - 1];
            mapIdProperty.stringValue = selectedMap.MapId;
            pointGuidProperty.stringValue = string.Empty;
            DrawPointPopup(pointGuidProperty, selectedMap.MapId);
            return;
        }

        int nextMapIndex = EditorGUILayout.Popup("Map", Mathf.Max(0, currentMapIndex), mapLabels);

        if (maps.Count > 0 && nextMapIndex >= 0 && nextMapIndex < maps.Count)
        {
            string nextMapId = maps[nextMapIndex].MapId;
            if (mapIdProperty.stringValue != nextMapId)
            {
                mapIdProperty.stringValue = nextMapId;
                pointGuidProperty.stringValue = string.Empty;
            }

            DrawPointPopup(pointGuidProperty, nextMapId);
        }
    }

    private void DrawPointPopup(SerializedProperty pointGuidProperty, string mapId)
    {
        MapPointScanCache cache = GetDatabase().PointScanCaches?.FirstOrDefault(item => item.MapId == mapId);
        List<MapPointScanData> points = cache?.Points?.Where(point => !string.IsNullOrWhiteSpace(point.PointGuid)).ToList()
            ?? new List<MapPointScanData>();

        if (points.Count == 0)
        {
            EditorGUILayout.TextField("PointGuid", pointGuidProperty.stringValue);
            EditorGUILayout.HelpBox("当前地图没有扫描缓存，先点击扫描全部点位。", MessageType.Warning);
            return;
        }

        string[] labels = points.Select(GetPointLabel).ToArray();
        int currentPointIndex = points.FindIndex(point => point.PointGuid == pointGuidProperty.stringValue);
        if (currentPointIndex < 0 && !string.IsNullOrWhiteSpace(pointGuidProperty.stringValue))
        {
            string[] missingPointLabels = new[] { $"缺失点位 ({ShortGuid(pointGuidProperty.stringValue)})" }.Concat(labels).ToArray();
            int selectedMissingIndex = EditorGUILayout.Popup("Point", 0, missingPointLabels);
            if (selectedMissingIndex == 0)
            {
                EditorGUILayout.HelpBox($"当前点位不在扫描缓存中：{pointGuidProperty.stringValue}", MessageType.Error);
                return;
            }

            pointGuidProperty.stringValue = points[selectedMissingIndex - 1].PointGuid;
            return;
        }

        int nextPointIndex = EditorGUILayout.Popup("Point", Mathf.Max(0, currentPointIndex), labels);

        if (nextPointIndex >= 0 && nextPointIndex < points.Count)
        {
            pointGuidProperty.stringValue = points[nextPointIndex].PointGuid;
        }

    }

    private void ScanAllMapPoints()
    {
        MapConnectionDatabaseSO database = GetDatabase();
        Scene activeScene = SceneManager.GetActiveScene();
        Object[] selection = Selection.objects;
        List<MapPointScanCache> caches = new List<MapPointScanCache>();
        List<string> scanWarnings = new List<string>();

        try
        {
            MapLinkPoint.SuppressAutoGeneratePointGuid = true;

            foreach (MapDefinitionSO map in database.MapRegistry.AllMaps.Where(map => map != null))
            {
                string scenePath = FindScenePath(map.SceneName);
                if (string.IsNullOrWhiteSpace(scenePath))
                {
                    caches.Add(new MapPointScanCache { MapId = map.MapId });
                    continue;
                }

                Scene loadedScene = FindLoadedScene(scenePath);
                bool openedByScanner = !loadedScene.IsValid();
                Scene openedScene = openedByScanner
                    ? EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive)
                    : loadedScene;

                MapPointScanCache cache = new MapPointScanCache { MapId = map.MapId };

                //扫描只读场景，不生成 PointGuid，也不保存场景
                foreach (MapLinkPoint point in Object.FindObjectsByType<MapLinkPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (point.gameObject.scene != openedScene)
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(point.PointGuid))
                    {
                        scanWarnings.Add($"扫描发现空 PointGuid，已跳过：Scene={map.SceneName}, GameObject={point.name}");
                        continue;
                    }

                    cache.Points.Add(new MapPointScanData
                    {
                        PointGuid = point.PointGuid,
                        DisplayName = point.DisplayName
                    });
                }

                caches.Add(cache);
                if (openedByScanner)
                {
                    EditorSceneManager.CloseScene(openedScene, true);
                }
            }
        }
        finally
        {
            MapLinkPoint.SuppressAutoGeneratePointGuid = false;

            if (activeScene.IsValid())
            {
                SceneManager.SetActiveScene(activeScene);
            }

            Selection.objects = selection;
        }

        Undo.RecordObject(database, "扫描地图点位");
        database.PointScanCaches.Clear();
        database.PointScanCaches.AddRange(caches);
        EditorUtility.SetDirty(database);
        ValidateDatabase();
        _validationWarnings.AddRange(scanWarnings);
    }

    private void ValidateDatabase()
    {
        _validationErrors.Clear();
        _validationWarnings.Clear();
        _hasValidated = true;

        MapConnectionDatabaseSO database = GetDatabase();
        MapRegistrySO registry = database.MapRegistry;

        if (registry == null)
        {
            _validationErrors.Add("MapRegistrySO 未配置。");
            Repaint();
            return;
        }

        ValidateMaps(registry);
        ValidateScanCaches(database);
        ValidateConnections(database);
        Repaint();
    }

    private void ValidateMaps(MapRegistrySO registry)
    {
        HashSet<string> mapIds = new HashSet<string>();
        foreach (MapDefinitionSO map in registry.AllMaps.Where(map => map != null))
        {
            if (string.IsNullOrWhiteSpace(map.MapId))
            {
                _validationErrors.Add($"{map.name} 的 MapId 为空。");
                continue;
            }

            if (!mapIds.Add(map.MapId))
            {
                _validationErrors.Add($"重复 MapId：{map.MapId}");
            }
        }
    }

    private void ValidateScanCaches(MapConnectionDatabaseSO database)
    {
        foreach (MapPointScanCache cache in database.PointScanCaches)
        {
            HashSet<string> pointGuids = new HashSet<string>();
            foreach (MapPointScanData point in cache.Points)
            {
                if (string.IsNullOrWhiteSpace(point.PointGuid))
                {
                    _validationWarnings.Add($"扫描缓存存在空 PointGuid：MapId={cache.MapId}");
                    continue;
                }

                if (!pointGuids.Add(point.PointGuid))
                {
                    _validationErrors.Add($"同一地图内重复 PointGuid：MapId={cache.MapId}, PointGuid={point.PointGuid}");
                }

                if (string.IsNullOrWhiteSpace(point.DisplayName))
                {
                    _validationWarnings.Add($"点位 DisplayName 为空：{GetPointLabel(point)}");
                }
            }
        }
    }

    private void ValidateConnections(MapConnectionDatabaseSO database)
    {
        HashSet<string> endpoints = new HashSet<string>();
        foreach (MapConnectionData connection in database.Connections)
        {
            ValidateEndpoint(database, connection.EndA, endpoints);
            ValidateEndpoint(database, connection.EndB, endpoints);
        }
    }

    private void ValidateEndpoint(MapConnectionDatabaseSO database, MapEndpointData endpoint, HashSet<string> endpoints)
    {
        if (string.IsNullOrWhiteSpace(endpoint.MapId) || string.IsNullOrWhiteSpace(endpoint.PointGuid))
        {
            _validationErrors.Add("连接端点缺少 MapId 或 PointGuid。");
            return;
        }

        if (!database.MapRegistry.AllMaps.Any(map => map != null && map.MapId == endpoint.MapId))
        {
            _validationErrors.Add($"端点引用了不存在的地图：{endpoint.MapId}");
        }

        MapPointScanCache cache = database.PointScanCaches.FirstOrDefault(item => item.MapId == endpoint.MapId);
        if (cache == null || !cache.Points.Any(point => point.PointGuid == endpoint.PointGuid))
        {
            _validationErrors.Add($"端点引用了不存在的点位：MapId={endpoint.MapId}, PointGuid={endpoint.PointGuid}");
        }

        string endpointKey = $"{endpoint.MapId}:{endpoint.PointGuid}";
        if (!endpoints.Add(endpointKey))
        {
            _validationErrors.Add($"一个端点出现在多条普通连接里：{endpointKey}");
        }
    }

    private void DrawValidationMessages()
    {
        foreach (string error in _validationErrors)
        {
            EditorGUILayout.HelpBox(error, MessageType.Error);
        }

        foreach (string warning in _validationWarnings)
        {
            EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }

        if (_hasValidated && _validationErrors.Count == 0 && _validationWarnings.Count == 0)
        {
            EditorGUILayout.HelpBox("校验通过。", MessageType.Info);
        }
    }

    private MapConnectionDatabaseSO GetDatabase()
    {
        return (MapConnectionDatabaseSO)target;
    }

    private static void ClearEndpoint(SerializedProperty endpointProperty)
    {
        endpointProperty.FindPropertyRelative("MapId").stringValue = string.Empty;
        endpointProperty.FindPropertyRelative("PointGuid").stringValue = string.Empty;
    }

    private static string GetMapLabel(MapDefinitionSO map)
    {
        string sceneName = string.IsNullOrWhiteSpace(map.SceneName) ? map.name : map.SceneName;
        return $"{sceneName} ({ShortGuid(map.MapId)})";
    }

    private static string GetPointLabel(MapPointScanData point)
    {
        string displayName = string.IsNullOrWhiteSpace(point.DisplayName) ? "未命名点" : point.DisplayName;
        return $"{displayName} ({ShortGuid(point.PointGuid)})";
    }

    private static string ShortGuid(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "空" : value.Substring(0, Mathf.Min(8, value.Length));
    }

    private static string FindScenePath(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return string.Empty;
        }

        foreach (string guid in AssetDatabase.FindAssets($"{sceneName} t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) == sceneName)
            {
                return path;
            }
        }

        return string.Empty;
    }

    private static Scene FindLoadedScene(string scenePath)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.path == scenePath)
            {
                return scene;
            }
        }

        return default;
    }
}
}
