using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerConfig_", menuName = "ScriptableObject/Config/PlayerConfig")]
public class PlayerConfigSO : ScriptableObject
{
    [Header("BASE DETAILS")]
    [SerializeField] private PlayerConfig _playerConfig;
    
    [Space(5)]
    [Header("FORM DETAILS")]
    //可以直接调用配置项中的这些数据
    [SerializeField] private PlayerFormConfig _formConfigUnarmed; 
    [SerializeField] private PlayerFormConfig _formConfigSword;
    
    //这个数据不能被调用 仅作为覆盖用
    //如果玩家有存档 那么玩家内部的可保存数据由存档来覆盖 
    //如果没有存档 则使用该配置项来覆盖
    [Space(5)]
    [Header("RUNNING DEFAULT DATA")]
    [Tooltip("这个是默认配置的玩家可保存数据")]
    [SerializeField] private PlayerSaveData _playerSaveData = new PlayerSaveData();
    
    
    public PlayerConfig PlayerConfig => _playerConfig;
    public PlayerFormConfig FormConfigUnarmed => _formConfigUnarmed;
    public PlayerFormConfig FormConfigSword => _formConfigSword;
    public PlayerSaveData PlayerSaveData => _playerSaveData;
#if UNITY_EDITOR
    //private void OnValidate()
    //{
    //    if(MaxHealth != 0)
    //        PlayerSaveData.CurrentHealth = MaxHealth;
    //}
#endif
}
/// <summary>
/// 玩家通用数据
/// </summary>
[Serializable]
public class PlayerConfig
{
    [SerializeField] private GameObject _playerPrefab;
    
    public GameObject PlayerPrefab => _playerPrefab;
    
}
[Serializable]
public class PlayerFormConfig : IMoveConfig, IHealthConfig, IAttackConfig
{
    [Header("BASIC DETAILS")] 
    [SerializeField] private int _maxHealthAmount;
    [Tooltip("限制玩家下落的最大速度 不会无限加速")]
    [SerializeField] private float _maxGravityScale = 10.0f;
    [Tooltip("该形态使用的动画状态机Controller")] 
    [SerializeField] private RuntimeAnimatorController _runtimeAnimatorController;

    [Space(1)]
    [Header("Move DETAILS")]
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _jumpSpeed = 18f;
    [Tooltip("跳跃次数")]
    [SerializeField] private int _jumpCount = 2;
    [SerializeField] private LayerMask _playerGroundLayerMask;
    [Tooltip("跳跃重力")]
    [SerializeField] private float _jumpGravityScale = 5.0f;
    [Tooltip("下落重力")]
    [SerializeField] private float _fallGravityScale = 7.0f;
    
    [Space(1)]
    [Header("HIT DETAILS")]
    [Tooltip("玩家受伤顿帧时间")]
    [SerializeField] private float _getHitStopTime = 0.3f;
    [Tooltip("玩家受伤击退力度")]
    [SerializeField] private float _getHitKnockbackForceValue = 15f;
    
    [Space(1)]
    [Header("ATTACK DETAILS")]
    [Tooltip("普通攻击的伤害值")] 
    [SerializeField] private int _attackDamage = 1;
    [Tooltip("普通攻击的检测层级")]
    [SerializeField] private LayerMask _attackCheckLayer;
    [Tooltip("普通攻击的检测中心偏移量")]
    [SerializeField] private Vector3 _attackCheckOffset;
    [Tooltip("普通攻击的检测盒子大小")]
    [SerializeField] private Vector3 _attackCheckBoundsSize;
    
    
    public RuntimeAnimatorController RuntimeAnimatorController => _runtimeAnimatorController;
    //--------------------- IMoveConfig ----------------------------
    public float RunSpeed => _runSpeed;
    public float JumpSpeed => _jumpSpeed;
    public int JumpCount => _jumpCount;
    public LayerMask PlayerGroundLayerMask => _playerGroundLayerMask;
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
