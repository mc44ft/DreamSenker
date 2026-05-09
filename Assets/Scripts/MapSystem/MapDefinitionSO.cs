using UnityEngine;
//单个关卡地图的SO文件

namespace DreamSenker.MapSystem
{
[CreateAssetMenu(fileName = "MapDefinition_", menuName = "ScriptableObject/Map/MapDefinition")]
public class MapDefinitionSO : ScriptableObject
{
    [field: SerializeField] public string MapId { get; private set; }
    [field: SerializeField] public string SceneName { get; private set; }
    [field: SerializeField] public float PlayerShadowDarknessStrength { get; private set; } = 2.5f;

    private void OnValidate()
    {
        //只在首次创建或旧数据为空时生成，避免重命名资产导致运行时 ID 改变
        if (string.IsNullOrWhiteSpace(MapId))
        {
            MapId = System.Guid.NewGuid().ToString("N");
        }
    }
}
}
