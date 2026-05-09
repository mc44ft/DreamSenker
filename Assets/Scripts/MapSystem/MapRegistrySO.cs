using System.Collections.Generic;
using UnityEngine;

//地图系统唯一地图清单，运行时和编辑器工具共用

namespace DreamSenker.MapSystem
{
[CreateAssetMenu(fileName = "MapRegistry_", menuName = "ScriptableObject/Map/MapRegistry")]
public class MapRegistrySO : ScriptableObject
{
    [field: SerializeField] public List<MapDefinitionSO> AllMaps { get; private set; } = new List<MapDefinitionSO>();
}
}
