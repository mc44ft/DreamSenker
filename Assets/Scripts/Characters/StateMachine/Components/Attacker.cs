
using PlayArk.StateMachine.Utilities;
using UnityEngine;

using DreamSeeker.Combat.Health;

using DreamSeeker.Characters;

namespace DreamSeeker.Characters.Player
{
[RequireComponent(typeof(Mover))]
public class Attacker : BaseComponent<IAttackConfig>
{
    private float _lastAttackTime = float.NegativeInfinity;
    private IAttackConfig _attackConfig;

    private Mover _mover;

    private void Awake()
    {
        _mover = GetComponent<Mover>();
    }

    /// <summary>
    /// 检查攻击冷却是否结束
    /// </summary>
    public bool IsCooldownReady => Time.time >= _lastAttackTime + _attackConfig.AttackCooldown;

    /// <summary>
    /// 触发攻击，由动画事件调用
    /// </summary>
    private void AttackAnimEvent()
    {
        //记录冷却
        StartAttackCooldown();
        
        //攻击检测
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            transform.position +
            Vector3.Scale(_attackConfig.AttackCheckOffset, new Vector3(_mover.FaceRight, 1, 1)),
            _attackConfig.AttackCheckBoundsSize,
            0f,
            _attackConfig.AttackCheckLayer);
        if (hits.Length > 0)
        {
            foreach (Collider2D hit in hits)
            {
                if(hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(
                        _attackConfig.AttackDamage,
                        new Vector2(_mover.FaceRight, 0));
                }
            }
        }
    }

    /// <summary>
    /// 记录攻击开始时间，由外部在攻击状态进入时调用
    /// </summary>
    private void StartAttackCooldown()
    {
        _lastAttackTime = Time.time;
    }

    public override void InjectionConfig(IAttackConfig config)
    {
        _attackConfig = config;
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.AttackCooldownReady:
                return IsCooldownReady;
        }
        return null;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_attackConfig == null)
            return;

        _mover ??= GetComponent<Mover>();
        if (_mover == null)
            return;

        //绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position +
            Vector3.Scale(_attackConfig.AttackCheckOffset, new Vector3(_mover.FaceRight, 1, 1)),
            _attackConfig.AttackCheckBoundsSize);
    }
#endif
}
}
