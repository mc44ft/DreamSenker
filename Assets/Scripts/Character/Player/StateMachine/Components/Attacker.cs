
using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(Mover))]
public class Attacker : BaseComponent<IAttackConfig>
{
    private bool _isAttacked;
    private IAttackConfig _attackConfig;
    
    private Mover _mover;

    private void Awake()
    {
        _mover = GetComponent<Mover>();
    }

    private void AttackAnimEvent()
    {
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
    
    public override void InjectionConfig(IAttackConfig config)
    {
        _attackConfig = config;
    }
    
    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        return null;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        //绘制玩家的攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position + 
            Vector3.Scale(_attackConfig.AttackCheckOffset, new Vector3(_mover.FaceRight, 1, 1)), 
            _attackConfig.AttackCheckBoundsSize);
    }
#endif
}
