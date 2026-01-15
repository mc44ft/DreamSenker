
using System;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
public class Mover : BaseComponent<IMoveConfig>
{
    //组件是独立的 不应该直接持有玩家管理器 应该持有一个通用的数据包
    //这个数据包由外部管理器依赖注入而来
    private IMoveConfig _moveConfig;
    private float _moveSpeedMultiplier = 1f;//移动的速度乘数
    private int _jumpCount;
    private bool _previousIsGround;
    private bool _isGround;
    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        CheckIsGround();
        ResetJumpCounter();
    }
    

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        _moveSpeedMultiplier = multiplier;
    }
    public void ResetJumpCounter()
    {
        if (_previousIsGround != _isGround)//接地状态改变
            if (_isGround)
                _jumpCount = _moveConfig.JumpCount;
    }
    public void CheckIsGround()
    {
        _previousIsGround = _isGround;
        //检测接地
        _isGround = Physics2D.OverlapBox(transform.position, new Vector2(0.6f, 0.05f), 0, _moveConfig.PlayerGroundLayerMask);
    }
    public void Move()
    {
        if (_moveConfig == null) return;
        
        //直接写在FixedUpdate里 不管什么状态 都可以移动
        _rigidbody.velocity = new Vector2(
            InputManager.Instance.HorizontalValue * _moveConfig.RunSpeed * _moveSpeedMultiplier,
            _rigidbody.velocity.y);
    }
    /// <summary>
    /// 跳跃的配置操作
    /// </summary>
    public void JumpEnterSetup()
    {
        //跳跃次数减一
        _jumpCount--;
        //设置重力
        _rigidbody.gravityScale = _moveConfig.JumpGravityScale;
        //设置跳跃速度
        _rigidbody.velocity = new Vector2(
            _rigidbody.velocity.x, _moveConfig.JumpSpeed);
    }
    
    public void FallEnterSetup()
    {
        //设置重力
        _rigidbody.gravityScale = _moveConfig.FallGravityScale;
    }
    public void StopMove()
    {
        _rigidbody.velocity = Vector2.zero;
    }
    /// <summary>
    /// 在玩家切换形态的时候 可更新配置文件
    /// </summary>
    public override void InjectionConfig(IMoveConfig moveConfig)
    {
        _moveConfig = moveConfig;
        _jumpCount = _moveConfig.JumpCount;
    }
    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.JumpCountNotZero:
                return _jumpCount != 0;
            case EPredicate.VerticalSpeedNotNegative:
                return _rigidbody.velocity.y > 0;
            case EPredicate.Grounded:
                return _isGround;
        }
        return null;
    }
}
