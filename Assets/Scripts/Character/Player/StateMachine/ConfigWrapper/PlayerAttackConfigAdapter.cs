

using UnityEngine;
/// <summary>
/// 使用 适配器模式
/// 所谓适配器 就是在这里面将IAttackConfig的数据再做进一步处理之后再返回
/// 将静态的PlayerFormConfig 和 动态的PlayerSaveData
/// 包装成一个统一的 IAttackConfig 接口 供Attacker组件使用
/// Attacker组件不需要考虑IAttackConfig接口中的数据是怎么来的 他只需要用就可以
/// 
/// IAttackConfig需要两份数据 共同作为支撑 所以使用这种方法
/// </summary>
public class PlayerAttackConfigAdapter : IAttackConfig
{
    private readonly IAttackConfig _staticConfig;
    private readonly PlayerSaveData _dynamicConfig;
    public PlayerAttackConfigAdapter(IAttackConfig staticConfig, PlayerSaveData dynamicConfig)
    {
        _staticConfig = staticConfig;
        _dynamicConfig = dynamicConfig;
    }
    //直接返回原数据
    public LayerMask AttackCheckLayer => _staticConfig.AttackCheckLayer;
    //返回经过处理的数据
    public int AttackDamage => (int)(_staticConfig.AttackDamage * _dynamicConfig.AttackMultiply);
    public Vector3 AttackCheckOffset => _staticConfig.AttackCheckOffset;
    public Vector3 AttackCheckBoundsSize => _staticConfig.AttackCheckBoundsSize;
    public float AttackCooldown => _staticConfig.AttackCooldown;
}