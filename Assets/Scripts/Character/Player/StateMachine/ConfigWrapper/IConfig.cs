using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 体现接口隔离原则
/// 将一个大的配置文件拆分成多个小的接口
/// 确保了像Mover这样的功能组件只依赖于它实际需要的最小数据子集
/// 这样不仅提高了组件的复用性
/// 也防止了不同形态之间的特有数据对逻辑执行层造成污染
/// 体现依赖倒置原则(DIP)
/// 通过引入一个接口层，让业务逻辑（组件）与数据来源（存档/配置）彻底解耦
/// </summary>
public interface IConfig
{
    
}

public interface IMoveConfig : IConfig
{
    public float RunSpeed { get; }
}

public interface IAttackConfig : IConfig
{
    public LayerMask AttackCheckLayer { get; }
    public int AttackDamage { get; }
    public Vector3 AttackCheckOffset { get; }
    public Vector3 AttackCheckBoundsSize { get; }
    public float AttackCooldown { get; }
}

public interface IHealthConfig : IConfig
{
    public int MaxHealthAmount { get; }
    public float GetHitKnockbackForceValue { get; }
    public float GetHitStopTime { get; }
}
public interface IJumpConfig : IConfig
{
    public float JumpSpeed { get; }
    public int JumpCount { get; }
    public LayerMask GroundLayerMask{ get; }
    public float JumpGravityScale { get; }
    public float FallGravityScale { get; }
}
public interface IFlyConfig
{
}
