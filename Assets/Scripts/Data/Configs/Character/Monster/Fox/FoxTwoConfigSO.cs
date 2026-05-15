using UnityEngine;

namespace DreamSeeker.Data.Configs.Character.Monster.Fox
{
[CreateAssetMenu(fileName = "FoxTwoConfig_", menuName = "ScriptableObject/Config/Fox/FoxTwoConfig")]
public class FoxTwoConfigSO : FoxBossConfigSO
{
    [field: Header("---------------------------------- FOX TWO DETAILS -------------------------------------")]
    // --- BASIC DETAILS ---
    [field: Header("BASIC DETAILS")]
    [field: Tooltip("范围攻击检测层级")]
    [field: SerializeField] 
    public LayerMask AttackLayerMask { get; private set; }
    [field: Tooltip("二尾形态死亡后 到一尾形态登场 之间的间隔时间")]
    [field: SerializeField]
    public float DeathIntervalTime { get; private set; } = 2f;

    // --- 跳起技能 ---
    [field: Header("SKILL JUMP DETAILS")]
    [field: Tooltip("跳起技能CD")]
    [field: SerializeField] 
    public float SkillJumpCooldown { get; private set; } = 5;
    [field: Tooltip("跳起技能攻击力 抓挠")]
    [field: SerializeField] 
    public int SkillJumpScratchDamage { get; private set; }
    [field: Tooltip("跳跃高度")]
    [field: SerializeField] 
    public float SkillJumpPower { get; private set; } = 3f;
    [field: Tooltip("跳跃持续时间")]
    [field: SerializeField] 
    public float SkillJumpDuration { get; private set; } = 1f;

    [field: Tooltip("跳跃曲线")]
    [field: SerializeField] 
    public AnimationCurve SkillJumpEaseCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);
    [field: Tooltip("跳跃接下落的停顿时间")]
    [field: SerializeField] 
    public float SkillJumpToFallIntervalTime { get; private set; } = 1f;
    [field: Tooltip("下落持续时间")]
    [field: SerializeField] 
    public float SkillJumpFallDuration { get; private set; } = 1f;
    [field: Tooltip("下落曲线")]
    [field: SerializeField] 
    public AnimationCurve SkillJumpFallEaseCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);
    [field: Tooltip("抓挠结束后需要一段停滞时间 至少设置为1")]
    [field: SerializeField]
    public float SkillJumpScratchIntervalTime { get; private set; } = 1;
    


    // --- 冲刺技能 ---
    [field: Header("SKILL DASH DETAILS")]
    [field: Tooltip("冲刺技能CD")]
    [field: SerializeField] 
    public float SkillDashCooldown { get; private set; } = 10;
    [field: Tooltip("冲刺次数")]
    [field: SerializeField] 
    public int SkillDashCount { get; private set; }
    [field: Tooltip("每次冲刺的间隔时间")]
    [field: SerializeField] 
    public float SkillDashIntervalTime { get; private set; }
    [field: Tooltip("冲刺距离")]
    [field: SerializeField] 
    public float SkillDashDistance { get; private set; } = 5f;

    [field: Tooltip("冲刺持续时间")]
    [field: SerializeField] 
    public float SkillDashDuration { get; private set; } = 0.3f;

    [field: Tooltip("冲刺曲线")]
    [field: SerializeField] 
    public AnimationCurve SkillDashEaseCurve { get; private set; } = AnimationCurve.Linear(0, 0, 1, 1);

    // --- 引爆技能 ---
    [field: Header("SKILL BOOM DETAILS")]
    [field: Tooltip("引爆技能CD")]
    [field: SerializeField] 
    public float SkillBoomCooldown { get; private set; } = 20;
    [field: Tooltip("火球预制体")]
    [field: SerializeField] 
    public GameObject SkillBoomFirePrefab { get; private set; }
    [field: Tooltip("火球个数")]
    [field: SerializeField] 
    public int SkillBoomFireCount { get; private set; } = 4;
    
    [field: Tooltip("火球蓄力时间")]
    [field: SerializeField] 
    public float SkillBoomFireLaunchChargeTime { get; private set; } = 3;
    [field: Tooltip("火球抛出高度")]
    [field: SerializeField] 
    public float SkillBoomFireLaunchPower { get; private set; } = 10;
    [field: Tooltip("火球发射过程总时间")]
    [field: SerializeField] 
    public float SkillBoomFireLaunchDuration { get; private set; } = 2;
    [field: Tooltip("火球发射随机落点精度偏移")]
    [field: SerializeField] 
    public float SkillBoomFireLaunchAccuracyOffset { get; private set; } = 2;
    [field: Tooltip("火球发射后的呆滞时间")]
    [field: SerializeField] 
    public float SkillBoomLaunchedTime { get; private set; } = 2;

    // --- 石化技能 ---
    [field: Header("SKILL PETRIFY DETAILS")]
    [field: Tooltip("石化技能CD")]
    [field: SerializeField]
    public float SkillPetrifyCooldown { get; private set; } = 10;
    [field: Tooltip("石化时间")]
    [field: SerializeField]
    public float SkillPetrifyTime { get; private set; } = 5;
    [field: Tooltip("石化技能后的呆滞时间")]
    [field: SerializeField]
    public float SkillPetrifyedTime { get; private set; } = 0;

    // --- 分身技能 ---
    [field: Header("SKILL CLONE DETAILS")]
    [field: Tooltip("分身技能CD")]
    [field: SerializeField]
    public float SkillCloneCooldown { get; private set; } = 40;
    [field: Tooltip("分身预制体")]
    [field: SerializeField]
    public GameObject SkillCloneObjectPrefab { get; private set; }
    [field: Tooltip("分身技能触发血量点 -- 百分比")]
    [field: Range(0, 1)]
    [field: SerializeField]
    public float SkillCloneTriggerHealthPercent { get; private set; }

    [field: Tooltip("分身生成间隔时间")]
    [field: SerializeField]
    public float SkillCloneObjectShowIntervalTime { get; private set; } = 1f;
    [field: Tooltip("分身持续时间")]
    [field: SerializeField]
    public float SkillCloneObjectKeepTime { get; private set; } = 10f;
    [field: Tooltip("分身消散间隔时间")]
    [field: SerializeField]
    public float SkillCloneDisappearIntervalTime { get; private set; } = 1f;
    [field: Tooltip("单个分身可回复血量")]
    [field: SerializeField]
    public int SkillCloneSingleRestoreHealthAmount { get; private set; } = 5;

}
}
