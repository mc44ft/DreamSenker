using UnityEngine;
//单个关卡地图的SO文件
[CreateAssetMenu(fileName = "MapDefinition_", menuName = "ScriptableObject/Map/MapDefinition")]
public class MapDefinitionSO : ScriptableObject
{
    [field: SerializeField] public string MapId { get; private set; }
    [field: SerializeField] public string SceneName { get; private set; }
    [field: SerializeField] public float PlayerShadowDarknessStrength { get; private set; } = 2.5f;
}
