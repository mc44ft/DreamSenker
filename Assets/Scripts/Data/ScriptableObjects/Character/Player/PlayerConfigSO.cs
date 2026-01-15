using System;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerConfig_", menuName = "ScriptableObject/Config/PlayerConfig")]
public class PlayerConfigSO : ScriptableObject
{
    [Header("BASE DETAILS")]
    [SerializeField] private PlayerConfig _playerConfig;

    [Space(5)]
    [Header("FORM DETAILS")]
    //可以直接调用配置项中的这些数据
    [SerializeField] private PlayerFormConfig _realFormConfig;
    [SerializeField] private PlayerFormConfig _mirrorFormConfig;
    
    //这个数据不能被调用 仅作为覆盖用
    //如果玩家有存档 那么玩家内部的可保存数据由存档来覆盖 
    //如果没有存档 则使用该配置项来覆盖
    [Space(5)]
    [Header("RUNNING DEFAULT DATA")]
    [Tooltip("这个是默认配置的玩家可保存数据")]
    [SerializeField] private PlayerSaveData _playerSaveData = new PlayerSaveData();
    
    
    public PlayerConfig PlayerConfig => _playerConfig;
    public PlayerFormConfig RealFormConfig => _realFormConfig;
    public PlayerFormConfig MirrorFormConfig => _mirrorFormConfig;
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
public class PlayerFormConfig : IMoveConfig, IHealthConfig
{
    [Header("BASIC DETAILS")] 
    [SerializeField] private int _maxHealthAmount;
    [Tooltip("限制玩家下落的最大速度 不会无限加速")]
    [SerializeField] private float _maxGravityScale = 10.0f;
    [Tooltip("该形态使用的动画状态机Controller")] 
    [SerializeField] private RuntimeAnimatorController _runtimeAnimatorController;
    [Tooltip("普通攻击的伤害值")] 
    [SerializeField] private int _attackDamage = 1;
    [Tooltip("普通攻击的检测层级")]
    [SerializeField] private LayerMask _attackCheckLayer;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _jumpSpeed = 18f;

    [Space(1)]
    [Header("ADVANCED DETAILS")]
    [Tooltip("跳跃次数")]
    [SerializeField] private int _jumpCount = 2;
    [Tooltip("玩家受伤顿帧时间")]
    [SerializeField] private float _getHitStopTime = 0.3f;
    [Tooltip("玩家受伤击退力度")]
    [SerializeField] private float _getHitKnockbackForceValue = 15f;
    [SerializeField] private LayerMask _playerGroundLayerMask;

    [Space(1)]
    [Header("PHYSIC DETAILS")]
    [Tooltip("跳跃重力")]
    [SerializeField] private float _jumpGravityScale = 5.0f;
    [Tooltip("下落重力")]
    [SerializeField] private float _fallGravityScale = 7.0f;
    
    [Space(1)]
    [Header("MIRROR FORM DETAILS")]
    [Tooltip("飞行时间")]
    [SerializeField] private int _flyingTime = 5;
    [Tooltip("飞行动力 0为悬浮")]
    [SerializeField] private float _flyingForce = 2.5f;
    [Tooltip("子弹预制体")]
    [SerializeField] private GameObject _ammoPrefab;
    [Tooltip("子弹速度")]
    [SerializeField] private float _ammoSpeed;
    [Tooltip("子弹伤害")]
    [SerializeField] private int _ammoDamage;
    [Tooltip("子弹射击间隔时间")]
    [SerializeField] private float _ammoFireInterval = 0.5f;
    
    public RuntimeAnimatorController RuntimeAnimatorController => _runtimeAnimatorController;
    public int AttackDamage => _attackDamage;
    public LayerMask AttackCheckLayer => _attackCheckLayer;
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
    
    public float MaxGravityScale => _maxGravityScale;
    public float GetHitStopTime => _getHitStopTime;
    
    
    
    public int FlyingTime => _flyingTime;
    public float FlyingForce => _flyingForce;
    public GameObject AmmoPrefab => _ammoPrefab;
    public float AmmoSpeed => _ammoSpeed;
    public int AmmoDamage => _ammoDamage;
    public float AmmoFireInterval => _ammoFireInterval;
}
