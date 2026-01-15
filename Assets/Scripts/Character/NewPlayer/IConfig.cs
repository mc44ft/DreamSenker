using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 利用接口隔离原则
/// 将一个大的配置文件拆分成多个小的接口
/// 确保了像Mover这样的功能组件只依赖于它实际需要的最小数据子集
/// 这样不仅提高了组件的复用性
/// 也防止了不同形态之间的特有数据对逻辑执行层造成污染
/// </summary>
public interface IConfig
{
    
}

public interface IMoveConfig : IConfig
{
    public float RunSpeed { get; }
    public float JumpSpeed { get; }
    public int JumpCount { get; }
    public LayerMask PlayerGroundLayerMask{ get; }
    public float JumpGravityScale { get; }
    public float FallGravityScale { get; }
}

public interface IAttackConfig : IConfig
{
    
}

public interface IHealthConfig : IConfig
{
    public int MaxHealthAmount { get; }
    public float GetHitKnockbackForceValue { get; }
}
public interface IJumpConfig
{
    
}
public interface IFlyConfig
{
}
