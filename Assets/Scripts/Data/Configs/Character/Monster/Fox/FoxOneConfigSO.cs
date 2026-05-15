using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSeeker.Data.Configs.Character.Monster.Fox
{
[CreateAssetMenu(fileName = "FoxOneConfig_", menuName = "ScriptableObject/Config/Fox/FoxOneConfig")]
public class FoxOneConfigSO : FoxBossConfigSO
{
    [field: Header("--------------------------- CLONE DETAILS ----------------------------")]
    [field: Tooltip("分身状态下的最大血量")]
    [field: SerializeField]
    public int CloneFormMaxHealthAmount { get; private set; } = 1;
}
}
