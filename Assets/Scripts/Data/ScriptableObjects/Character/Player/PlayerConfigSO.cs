using System;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerConfig_", menuName = "ScriptableObject/Config/PlayerConfig")]
public class PlayerConfigSO : ScriptableObject
{
    [field: Header("BASE DETAILS")]
    [field: SerializeField] 
    public GameObject PlayerPrefab { get; private set; }
    [field: SerializeField] 
    public LayerMask PlayerGroundLayerMask { get; private set; }

    [field: SerializeField] 
    public int MaxHealthAmount { get; private set; }

    [field: Tooltip("限制玩家下落的最大速度 不会无限加速")]
    [field: SerializeField] 
    public float MaxGravityScale { get; private set; } = 10.0f;



    [field: Space(5)]
    [field: Header("FORM DETAILS")]
    //可以直接调用配置项中的这些数据
    [field: SerializeField] 
    public PlayerConfig RealFormConfig { get; private set; }

    [field: SerializeField] 
    public PlayerConfig MirrorFormConfig { get; private set; }

    //这个数据不能被调用 仅作为覆盖用
    //如果玩家有存档 那么玩家内部的可保存数据由存档来覆盖 
    //如果没有存档 则使用该配置项来覆盖
    [field: Space(5)]
    [field: Header("RUNNING DEFAULT DATA")]
    [field: Tooltip("这个是默认配置的玩家可保存数据")]
    [field: SerializeField] 
    public PlayerSaveData PlayerSaveData { get; private set; } = new PlayerSaveData();
#if UNITY_EDITOR
    //private void OnValidate()
    //{
    //    if(MaxHealth != 0)
    //        PlayerSaveData.CurrentHealth = MaxHealth;
    //}
#endif
}
[Serializable]
public class PlayerConfig
{
    [field: Header("BASIC DETAILS")]
    [field: Tooltip("该形态使用的动画状态机Controller")]
    [field: SerializeField]
    public RuntimeAnimatorController RuntimeAnimatorController { get; private set; }

    [field: Tooltip("普通攻击的伤害值")]
    [field: SerializeField]
    public int AttackDamage { get; private set; } = 1;
    [field: Tooltip("普通攻击的检测层级")]
    [field: SerializeField]
    public LayerMask AttackCheckLayer { get; private set; }


    [field: SerializeField]
    public float RunSpeed { get; private set; } = 8;

    [field: SerializeField]
    public float JumpSpeed { get; private set; } = 18;


    [field: Space(1)]
    [field: Header("ADVANCED DETAILS")]
    [field: Tooltip("跳跃次数")]
    [field: SerializeField] 
    public int JumpCount { get; private set; } = 2;
    


    [field: Tooltip("玩家受伤顿帧时间")]
    [field: SerializeField] 
    public float GetHitStopTime { get; private set; } = 0.3f;

    [field: Tooltip("玩家受伤击退力度")]
    [field: SerializeField] 
    public float GetHitKnockbackForceValue { get; private set; } = 15f;

    [field: Space(1)]
    [field: Header("PHYSIC DETAILS")]
    [field: Tooltip("跳跃重力")]
    [field: SerializeField] 
    public float JumpGravityScale { get; private set; } = 5.0f;

    [field: Tooltip("下落重力")]
    [field: SerializeField] 
    public float FallGravityScale { get; private set; } = 7.0f;

    [field: Space(1)]
    [field: Header("MIRROR FORM DETAILS")]
    [field: Tooltip("飞行时间")]
    [field: SerializeField]
    public int FlyingTime { get; private set; } = 5;
    [field: Tooltip("飞行动力 0为悬浮")]
    [field: SerializeField]
    public float FlyingForce { get; private set; } = 2.5f;
    [field: Tooltip("子弹预制体")]
    [field: SerializeField]
    public GameObject AmmoPrefab { get; private set; }
    [field: Tooltip("子弹速度")]
    [field: SerializeField]
    public float AmmoSpeed { get; private set; }
    [field: Tooltip("子弹伤害")]
    [field: SerializeField]
    public int AmmoDamage { get; private set; }
    [field: Tooltip("子弹射击间隔时间")]
    [field: SerializeField]
    public float AmmoFireInterval { get; private set; } = 0.5f;

}
