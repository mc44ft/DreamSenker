using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ScriptableObjects.Character.Monster.Fox
{
//[CreateAssetMenu(fileName = "FoxBossConfig_", menuName = "ScriptableObject/Config/Fox/FoxBossConfig")]
public class FoxBossConfigSO : ScriptableObject
{
    // --- BASIC DETAILS ---
    [field: Header("----------------------------- BASE DETAILS ---------------------------------")]
    [field: Tooltip("生命值")]
    [field: SerializeField]
    public int MaxHealthAmount { get; private set; }
    [field: Tooltip("触碰伤害")]
    [field: SerializeField]
    public int TouchDamage { get; private set; }

    [field: Header("BASE HIDE DETAILS")]
    [field: Tooltip("隐匿效果过渡时间")]
    [field: SerializeField]
    public float HideDuration { get; private set; } = 0.5f;
    [field: Tooltip("隐匿最小时间 超过这个时间后 技能CD恢复即现身")]
    [field: SerializeField]
    public float HideMinTime { get; private set; } = 0.5f;

    [field: Header("BASE SHOW DETAILS")]
    [field: Tooltip("现身效果过渡时间")]
    [field: SerializeField]
    public float ShowDuration { get; private set; } = 0.5f;

    [field: Header("BASE FLOATING DETAILS")]
    [field: Tooltip("悬浮偏移量")]
    [field: SerializeField]
    public float FloatingOffset { get; private set; } = 0.5f;
    [field: Tooltip("悬浮单次循环时间")]
    [field: SerializeField]
    public float FloatingOneLoopTime { get; private set; } = 1f;
}
}
