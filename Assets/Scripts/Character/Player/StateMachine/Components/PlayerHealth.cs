using PlayArk.StateMachine.Utilities;
using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : BaseComponent<IHealthConfig>, IDamageable
{
    [Tooltip("受击后的无敌时间")]
    [SerializeField] private float _invulnerableTime = 0.5f;

    public int MaxHealthAmount => _health.MaxHealthAmount;
    public int CurrentHealthAmount => _health.CurrentHealthAmount;

    private Health _health;
    private IHealthConfig _healthConfig;
    private Vector2 _getHitDirection;
    private float _invulnerableTimer;
    private readonly LazyEvent _onDamageTaken = new LazyEvent();
    private Rigidbody2D _rb;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _invulnerableTimer -= Time.deltaTime;
    }

    public void Initialize(int maxHealthAmount, int currentHealthAmount)
    {
        _health.Initialize(maxHealthAmount, currentHealthAmount);
        TriggerPlayerHealthUpdate();
    }

    public void ApplyDamage(int damage)
    {
        _health.ApplyDamage(damage);
        TriggerPlayerHealthUpdate();
    }

    public void RestoreHealth(int healthAmount)
    {
        _health.RestoreHealth(healthAmount);
        TriggerPlayerHealthUpdate();
    }

    public void DoKnockback()
    {
        // 受击时沿攻击方向反向击退。
        HelperUtilities.DoKnockback(_rb, _getHitDirection,
            _healthConfig.GetHitKnockbackForceValue);
    }

    public override void InjectionConfig(IHealthConfig config)
    {
        _healthConfig = config;
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
        // 防止同一段攻击在短时间内重复扣血。
        if (_invulnerableTimer > 0)
            return;

        _onDamageTaken.StratInvoke();
        _invulnerableTimer = _invulnerableTime;

        _getHitDirection = attackDirection;
        ApplyDamage(damage);
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
    /// 把受击效果嵌入到动画内部
    /// </summary>
    private void GetHitAnimEvent()
    {
        if (!this.enabled) return;
        GameManager.Instance.CameraShake();
        GameManager.Instance.DoHitStop(_healthConfig.GetHitStopTime);
    }

    private void TriggerPlayerHealthUpdate()
    {
        // 玩家血条 UI 仍通过事件中心刷新。
        EventCenter.Instance.EventTrigger(
            E_EventType.Player_HealthUpdate,
            this,
            new PlayerHealthUpdateEventArgs(MaxHealthAmount, CurrentHealthAmount));
    }
}
