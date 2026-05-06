using PlayArk.StateMachine.Utilities;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Jumper : BaseComponent<IJumpConfig>
{
    //跳跃配置由形态策略通过现有依赖注入流程传入
    private IJumpConfig _jumpConfig;
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

    public void ResetJumpCounter()
    {
        if (_jumpConfig == null) return;

        if (_previousIsGround != _isGround)//接地状态改变
            if (_isGround)
                _jumpCount = _jumpConfig.JumpCount;
    }

    public void CheckIsGround()
    {
        if (_jumpConfig == null) return;

        _previousIsGround = _isGround;
        //检测接地
        _isGround = Physics2D.OverlapBox(transform.position, new Vector2(0.6f, 0.05f), 0, _jumpConfig.GroundLayerMask);
    }

    /// <summary>
    /// 跳跃的配置操作
    /// </summary>
    public void JumpEnterSetup()
    {
        if (_jumpConfig == null) return;

        //跳跃次数减一
        _jumpCount--;
        //设置重力
        _rigidbody.gravityScale = _jumpConfig.JumpGravityScale;
        //设置跳跃速度
        _rigidbody.velocity = new Vector2(
            _rigidbody.velocity.x, _jumpConfig.JumpSpeed);
    }

    public void FallEnterSetup()
    {
        if (_jumpConfig == null) return;

        //设置下落重力
        _rigidbody.gravityScale = _jumpConfig.FallGravityScale;
    }

    /// <summary>
    /// 在玩家切换形态的时候 可更新配置文件
    /// </summary>
    public override void InjectionConfig(IJumpConfig jumpConfig)
    {
        _jumpConfig = jumpConfig;
        _jumpCount = _jumpConfig.JumpCount;
    }

    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        switch (predicate)
        {
            case EPredicate.JumpCountNotZero:
                return _jumpConfig != null && _jumpCount != 0;
            case EPredicate.VerticalSpeedNotNegative:
                return _rigidbody.velocity.y > 0;
            case EPredicate.Grounded:
                return _jumpConfig != null && _isGround;
        }

        return null;
    }
}
