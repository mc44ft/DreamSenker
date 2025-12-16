using UnityEngine;
[CreateAssetMenu(fileName = "SpiderConfig_", menuName = "ScriptableObject/Config/SpiderConfig")]
public class SpiderConfigSO : ScriptableObject
{
    [field: Header("BASIC DETAILS")]

    [field: Tooltip("正常爬行速度")]
    [field: SerializeField] 
    public float CrawlSpeed { get; private set; }

    [field: Tooltip("爬行到目标点后的待机时间")]
    [field: SerializeField]
    public float IdleTime { get; private set; }
    [field: Tooltip("血量")]
    [field: SerializeField]
    public int MaxHealthAmount { get; private set; }
    [field: Tooltip("触碰伤害")]
    [field: SerializeField]
    public int TouchDamage { get; private set; }


    [field: Space(1)]
    [field: Header("SKILL HEAL")]

    [field: Tooltip("技能一冷却时间")]
    [field: SerializeField]
    public float SkillHealCooldown { get; private set; }
    [field: Tooltip("回血量")]
    [field: SerializeField]
    public int HealAmount { get; private set; }
    [field: Tooltip("回血前摇时间")]
    [field: SerializeField]
    public float HealWindupTime { get; private set; }

    [field: Space(1)]
    [field: Header("SKILL VENOM BITE")]
    [field: Tooltip("技能二冷却时间")]
    [field: SerializeField]
    public float SkillVenomBiteCooldown { get; private set; }
    [field: Tooltip("毒伤瞬间伤害")]
    [field: SerializeField]
    public int SkillVenomBiteDamage { get; private set; }
    [field: Tooltip("毒伤持续伤害")]
    [field: SerializeField]
    public int SkillVenomBiteContinueDamage { get; private set; }
    [field: Tooltip("毒伤持续伤害间隔时间")]
    [field: SerializeField]
    public float SkillVenomBiteContinueDamageIntervalTime { get; private set; }
    [field: Tooltip("毒伤持续时间")]
    [field: SerializeField]
    public float SkillVenomBiteContinueTime { get; private set; }
    [field: Tooltip("追击速度")]
    [field: SerializeField]
    public float ChaseSpeed { get; private set; }

    [field: Space(1)]
    [field: Header("SKILL WEB")]
    [field: Tooltip("技能三冷却时间")]
    [field: SerializeField]
    public float SkillWebCooldown { get; private set; }
    [field: Tooltip("蜘蛛丝预制体")]
    [field: SerializeField]
    public GameObject SpiderSilkPrefab { get; private set; }
    [field: Tooltip("吐丝前摇时间")]
    [field: SerializeField]
    public float SilkWindupTime { get; private set; } = 1f;
    [field: Tooltip("蜘蛛丝持续时间")]
    [field: SerializeField]
    public float SilkContinueTime { get; private set; } = 5f;
    [field: Tooltip("蜘蛛丝速度")]
    [field: SerializeField]
    public float SilkSpeed { get; private set; } = 5f;
    [field: Tooltip("蜘蛛丝限制玩家移速比率")]
    [field: SerializeField]
    public float SilkClampSpeedRate { get; private set; } = 0.5f;
    [field: Tooltip("蜘蛛丝生命值 玩家攻击几次可破坏")]
    [field: SerializeField]
    public int SilkHealthAmount { get; private set; } = 3;

    [field: Space(1)]
    [field: Header("SKILL SHAKE")]
    [field: Tooltip("技能四冷却时间")]
    [field: SerializeField]
    public float SkillShakeCooldown { get; private set; }
    [field: Tooltip("技能四前摇时间")]
    [field: SerializeField]
    public float SkillShakeWindUpTime { get; private set; } = 0f;
    [field: Tooltip("每一段击打的间隔时间 最后一个为技能结束后的呆滞时间")]
    [field: SerializeField]
    public float[] WebHitIntervalTimeArray { get; private set; }
    [field: Tooltip("每次跺脚的镜头震动力度 ")]
    [field: SerializeField]
    public float[] WebHitCameraShakeForceArray { get; private set; }
    [field: Tooltip("每一段攻击的伤害 ")]
    [field: SerializeField]
    public int[] WebHitDamageArray { get; private set; }
    [field: Tooltip("返回中心点的速度 返回中心点后 开始技能")]
    [field: SerializeField]
    public float ReturnCenterSpeed { get; private set; }


#if UNITY_EDITOR
    private void OnValidate()
    {
        if(!(WebHitIntervalTimeArray.Length == WebHitCameraShakeForceArray.Length &&
            WebHitCameraShakeForceArray.Length == WebHitDamageArray.Length))
        {
            Debug.LogWarning("四技能的三个数组长度必须相等！");
        }
    }
#endif
}
