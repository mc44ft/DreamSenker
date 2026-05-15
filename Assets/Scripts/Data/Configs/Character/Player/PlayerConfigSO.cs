using System;
using UnityEngine;

using DreamSeeker.Data.Runtime;

using DreamSeeker.Characters;

namespace DreamSeeker.Data.Configs.Character.Player
{
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "ScriptableObject/Config/PlayerConfig")]
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
public class PlayerFormConfig : IMoveConfig, IJumpConfig, IHealthConfig, IAttackConfig
{
    [Header("最大生命值")] 
    [SerializeField] private int _maxHealthAmount;
    [Header("限制下落的最大速度 不会无限加速")]
    [SerializeField] private float _maxGravityScale = 10.0f;
    [Header("该形态使用的动画状态机Controller")] 
    [SerializeField] private RuntimeAnimatorController _runtimeAnimatorController;

    [Space(1)]
    [Header("移动速度")]
    [SerializeField] private float _runSpeed = 8f;

    [Space(1)]
    [Header("Jump DETAILS")]
    [SerializeField] private float _jumpSpeed = 18f;
    [Header("跳跃次数")]
    [SerializeField] private int _jumpCount = 2;
    [Header("接地层级")]
    [SerializeField] private LayerMask _playerGroundLayerMask;
    [Header("跳跃重力")]
    [SerializeField] private float _jumpGravityScale = 5.0f;
    [Header("下落重力")]
    [SerializeField] private float _fallGravityScale = 7.0f;
    
    [Space(1)]
    [Header("受伤顿帧时间")]
    [SerializeField] private float _getHitStopTime = 0.3f;
    [Header("受伤击退力度")]
    [SerializeField] private float _getHitKnockbackForceValue = 15f;
    
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
    
    
    public RuntimeAnimatorController RuntimeAnimatorController => _runtimeAnimatorController;
    //--------------------- IMoveConfig ----------------------------
    public float RunSpeed => _runSpeed;
    //--------------------- IJumpConfig ----------------------------
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
    public float AttackCooldown => _attackCooldown;

    public float MaxGravityScale => _maxGravityScale;
}
}
