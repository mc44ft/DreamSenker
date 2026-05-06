using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.ScriptableObjects.Character.Monster.Fox
{
[CreateAssetMenu(fileName = "FoxBoomFireConfig_", menuName = "ScriptableObject/Config/Fox/FoxBoomFireConfig")]
public class FoxBoomFireConfigSO : ScriptableObject
{
    // --- BASIC DETAILS ---
    [field: Header("BASIC DETAILS")]
    [field: Tooltip("爆炸伤害")]
    [field: SerializeField] public int BoomDamage { get; private set; } = 5;
    [field: Tooltip("生成到开始爆炸的时间")]
    [field: SerializeField] public float DetonateTime { get; private set; } = 10f;
    [field: Tooltip("引爆时间随机偏移范围 + -")]
    [field: SerializeField] public float DetonateTimeOffsetRange { get; private set; } = 2f;
    [field: Tooltip("动画加速最大值")]
    [field: SerializeField] public float AnimationMaxSpeed { get; private set; } = 10f;
    [field: Tooltip("动画开始抖动时间（倒计时）")]
    [field: SerializeField] public float AnimationShakeStartTimeDown { get; private set; } = 1f;
    [field: Tooltip("引爆放大倍数")]
    [field: SerializeField] public float BoomScaleMultiply { get; private set; } = 4f;
    [field: Tooltip("引爆放大过渡时间")]
    [field: SerializeField] public float BoomScaleDuration { get; private set; } = 0.1f;
    [field: Tooltip("余波消散过渡时间")]
    [field: SerializeField] public float BoomFadeToZeroDuration { get; private set; } = 0.2f;
    
}
}
