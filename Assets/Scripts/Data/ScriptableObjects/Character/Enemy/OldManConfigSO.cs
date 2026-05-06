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
    public class OldManConfig : IMoveConfig, IAttackConfig, IHealthConfig
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
        [Header("跳跃力度")]
        [SerializeField] private float _jumpSpeed = 18f;
        [Header("跳跃次数")]
        [SerializeField] private int _jumpCount = 2;
        [SerializeField] private LayerMask _playerGroundLayerMask;
        [Header("跳跃重力")]
        [SerializeField] private float _jumpGravityScale = 5.0f;
        [Header("下落重力")]
        [SerializeField] private float _fallGravityScale = 7.0f;
        
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
        
        //--------------------- IMoveConfig ----------------------------
        public float RunSpeed => _runSpeed;
        public float JumpSpeed => _jumpSpeed;
        public int JumpCount => _jumpCount;
        public LayerMask GroundLayerMask => _playerGroundLayerMask;
        public float JumpGravityScale => _jumpGravityScale;
        public float FallGravityScale => _fallGravityScale;
        //--------------------- IHealthConfig ----------------------------
        public int MaxHealthAmount => _maxHealthAmount;
        public float GetHitKnockbackForceValue => _getHitKnockbackForceValue;
        public float GetHitStopTime => _getHitStopTime;
        //--------------------- IAttackConfig ----------------------------
        public LayerMask AttackCheckLayer => _attackCheckLayer;
        public int AttackDamage => _attackDamage;
        public Vector3 AttackCheckOffset => _attackCheckOffset;
        public Vector3 AttackCheckBoundsSize => _attackCheckBoundsSize;

        public float MaxGravityScale => _maxGravityScale;
    }
}