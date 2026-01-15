using System;
using System.Collections;
using System.Collections.Generic;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class NewHealth : BaseComponent<IHealthConfig>, IDamageable
{
    public int MaxHealthAmount => _maxHealthAmount;
    public int CurrentHealthAmount => _currentHealthAmount;
    
    private IHealthConfig _healthConfig;
    private int _maxHealthAmount;
    private int _currentHealthAmount;
    private Vector2 _getHitDirection;
    
    private bool _isDead = false;
    //无敌时间
    private readonly float _invulnerableTime = 0.5f;
    private float _invulnerableTimer = 0;
    
    private readonly LazyEvent _onDamageTaken = new LazyEvent();
    private readonly LazyEvent _onDie = new LazyEvent();
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _invulnerableTimer -= Time.deltaTime;
    }

    public void Initialize(int maxHealthAmount, int currentHealthAmount)
    {
        _isDead = false;
        _maxHealthAmount = maxHealthAmount;
        _currentHealthAmount = currentHealthAmount;
    }
    public void ApplyDamage(int damage)
    {
        _currentHealthAmount -= damage;
        if (_currentHealthAmount <= 0 )
        {
            Death();
        }
    }
    public void RestoreHealth(int healthAmount)
    {
        _currentHealthAmount = Mathf.Min(_currentHealthAmount + healthAmount, _maxHealthAmount);
    }
    private void Death()
    {
        _isDead = true;
    }

    public void DoKnockback()
    {
        //反方向击飞
        //这里优化成斜上角
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
                return _isDead;
        }
        return null;
    }
    
    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        //防止同时多次伤害 给一个无敌时间
        if (_invulnerableTimer > 0)
            return;
        _onDamageTaken.StratInvoke();
        _invulnerableTimer = _invulnerableTime;
        
        //更新本次冲击方向
        _getHitDirection = attackDirection;
        ApplyDamage(damage);

        //通知外部UI更新血条
        EventCenter.Instance.EventTrigger(
            E_EventType.Player_HealthUpdate, 
            this, 
            new PlayerHealthUpdateEventArgs(_maxHealthAmount, _currentHealthAmount));
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
}
