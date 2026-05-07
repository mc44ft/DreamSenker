using UnityEngine;

namespace Data.ScriptableObjects.Character.Enemy
{
    [CreateAssetMenu(fileName = "OldManConfig", menuName = "ScriptableObject/Config/Enemy/OldManConfig")]
    public class OldManConfigSO :  ScriptableObject
    {
        [SerializeField] private OldManConfig _config;
        public OldManConfig Config => _config;
    }

    [System.Serializable]
    public class OldManConfig : IMoveConfig, IAttackConfig, IHealthConfig, IEnemyAIConfig
    {
        [Header("限制下落的最大速度 不会无限加速")]
        [SerializeField] private float _maxGravityScale = 10.0f;

        //--------------------- IHealthConfig ----------------------------
        [Header("最大生命值")]
        [SerializeField] private int _maxHealthAmount;
        [Header("受伤顿帧时间")]
        [SerializeField] private float _getHitStopTime = 0.3f;
        [Header("受伤击退力度")]
        [SerializeField] private float _getHitKnockbackForceValue = 15f;
        //--------------------- IMoveConfig ----------------------------
        [Space(1)]
        [Header("奔跑速度")]
        [SerializeField] private float _runSpeed = 8f;

        //--------------------- IAttackConfig ----------------------------
        [Space(1)]
        [Header("普通攻击的伤害值")]
        [SerializeField] private int _attackDamage = 1;
        [Header("普通攻击的检测层级")]
        [SerializeField] private LayerMask _attackCheckLayer;
        [Header("普通攻击的检测中心偏移量")]
        [SerializeField] private Vector3 _attackCheckOffset;
        [Header("普通攻击的检测盒子大小")]
        [SerializeField] private Vector3 _attackCheckBoundsSize;
        [Header("普通攻击冷却时间")]
        [SerializeField] private float _attackCooldown = 0.5f;

        //--------------------- IEnemyAIConfig ----------------------------
        [Space(1)]
        [Header("检测到玩家的距离")]
        [SerializeField] private float _detectRange = 5f;
        [Header("进入攻击范围的距离")]
        [SerializeField] private float _attackRange = 1.5f;
        [Header("丢失目标的距离")]
        [SerializeField] private float _loseTargetRange = 8f;

        //--------------------- IMoveConfig ----------------------------
        public float RunSpeed => _runSpeed;
        //--------------------- IHealthConfig ----------------------------
        public int MaxHealthAmount => _maxHealthAmount;
        public float GetHitKnockbackForceValue => _getHitKnockbackForceValue;
        public float GetHitStopTime => _getHitStopTime;
        //--------------------- IAttackConfig ----------------------------
        public LayerMask AttackCheckLayer => _attackCheckLayer;
        public int AttackDamage => _attackDamage;
        public Vector3 AttackCheckOffset => _attackCheckOffset;
        public Vector3 AttackCheckBoundsSize => _attackCheckBoundsSize;
        public float AttackCooldown => _attackCooldown;
        //--------------------- IEnemyAIConfig ----------------------------
        public float DetectRange => _detectRange;
        public float AttackRange => _attackRange;
        public float LoseTargetRange => _loseTargetRange;

        public float MaxGravityScale => _maxGravityScale;
    }
}