using UnityEngine;
//单个关卡地图的SO文件

namespace DreamSenker.MapSystem.Data
{
[CreateAssetMenu(fileName = "MapDefinition_", menuName = "ScriptableObject/Map/MapDefinition")]
public class MapDefinitionSO : ScriptableObject
{
    [field: ReadOnly]
    [field: SerializeField] public string MapId { get; private set; }
    [field: SerializeField] public string SceneName { get; private set; }
    [field: Header("玩家阴影特效系数")]
    [field: SerializeField] public float PlayerShadowDarknessStrength { get; private set; } = 2.5f;
    [field: Header("Follow 虚拟相机日常战斗正交视野大小")]
    [field: SerializeField] public float FollowCameraOrthoSize { get; private set; } = 2.5f;
    [field: Header("Follow 虚拟相机Boss战正交视野大小")]
    [field: SerializeField] public float FollowCameraBossOrthoSize { get; private set; } = 6f;
    [field: Header("Dialogue 虚拟相机对话正交视野大小")]
    [field: SerializeField] public float DialogueCameraOrthoSize { get; private set; } = 6f;
    [field: Header("Dialogue 虚拟相机对话正交视野偏移Y值")]
    [field: SerializeField] public float DialogueCameraOffsetY { get; private set; } = 6f;
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
