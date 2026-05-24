using PlayArk.StateMachine.Utilities;
using UnityEngine;

using DreamSeeker.Combat.Health;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

using DreamSeeker.Characters;

/// <summary>
/// 通用受击组件
/// 提供无敌时间、击退、顿帧等受击效果
/// </summary>

namespace DreamSeeker.Characters.Player
{
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class DamageableHealth : BaseComponent<IHealthConfig>, IDamageable
{
    [Tooltip("受击后的无敌时间")]
    [SerializeField] private float _invulnerableTime = 0.5f;
    [Tooltip("是否限制最低血量，默认关闭以保持普通死亡流程")]
    [SerializeField] private bool _limitMinHealth = false;
    [Tooltip("开启最低血量限制时，血量最低降到该值")]
    [SerializeField] private int _minHealth = 1;

    public int MaxHealthAmount => _health.MaxHealthAmount;
    public int CurrentHealthAmount => _health.CurrentHealthAmount;

    private Health _health;
    private IHealthConfig _healthConfig;
    private NpcModeController _npcModeController;
    private Vector2 _getHitDirection;
    private float _invulnerableTimer;
    private readonly LazyEvent _onDamageTaken = new LazyEvent();
    private Rigidbody2D _rb;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
        // 可选缓存，玩家、Boss、纯战斗敌人可以没有 NpcModeController。
        _npcModeController = GetComponent<NpcModeController>();
    }

    private void Update()
    {
        _invulnerableTimer -= Time.deltaTime;
    }

    public void Initialize(int maxHealthAmount, int currentHealthAmount)
    {
        _health.Initialize(maxHealthAmount, currentHealthAmount);
    }

    public void ApplyDamage(int damage)
    {
        if (ShouldLimitMinHealth())
        {
            _health.ApplyDamageWithMinHealth(damage, _minHealth);
            return;
        }

        _health.ApplyDamage(damage);
    }

    public void RestoreHealth(int healthAmount)
    {
        _health.RestoreHealth(healthAmount);
    }

    /// <summary>
    /// 受击时沿攻击方向反向击退
    /// </summary>
    public void DoKnockback()
    {
        HelperUtilities.DoKnockback(_rb, _getHitDirection,
            _healthConfig.GetHitKnockbackForceValue);
    }

    public override void InjectionConfig(IHealthConfig config)
    {
        _healthConfig = config;
        Initialize(config.MaxHealthAmount, config.MaxHealthAmount);
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.TakeDamage:
                return _onDamageTaken.WasInvoked();
            case EPredicate.Dead:
                return _health.IsDead;
        }
        return null;
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        // 防止同一段攻击在短时间内重复扣血
        if (_invulnerableTimer > 0)
            return;

        // 友好状态被攻击时，条件不满足则本次攻击完全无效。
        if (_npcModeController != null && _npcModeController.IsFriendly && !_npcModeController.TrySwitchToHostile())
        {
            return;
        }

        _onDamageTaken.StratInvoke();
        _invulnerableTimer = _invulnerableTime;

        _getHitDirection = attackDirection;
        ApplyDamage(damage);

        TrySwitchToFriendlyWhenReachMinHealth();
    }

    /// <summary>
    /// 判断当前扣血是否需要限制最低血量。
    /// </summary>
    private bool ShouldLimitMinHealth()
    {
        return _limitMinHealth && _minHealth > 0;
    }

    /// <summary>
    /// 血量到达最低值且对象仍存活时，尝试从敌对切回友好。
    /// </summary>
    private void TrySwitchToFriendlyWhenReachMinHealth()
    {
        if (_npcModeController == null || !_npcModeController.IsHostile)
        {
            return;
        }

        if (!ShouldLimitMinHealth() || _health.IsDead || _health.CurrentHealthAmount > _minHealth)
        {
            return;
        }

        _npcModeController.TrySwitchToFriendlyAfterDefeat();
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void Select()
    {

    }

    public void Deselect()
    {

    }

    /// <summary>
    /// 受击顿帧效果，由动画事件调用
    /// </summary>
    private void GetHitAnimEvent()
    {
        if (!this.enabled) return;
        GameManager.Instance.CameraShake();
        GameManager.Instance.DoHitStop(_healthConfig.GetHitStopTime);
    }
}
}
