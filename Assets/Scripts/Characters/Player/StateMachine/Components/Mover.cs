
using System;
using PlayArk.StateMachine.Utilities;
using UnityEngine;


using DreamSenker.Characters;

namespace DreamSenker.Characters.Player
{
[RequireComponent(typeof(Rigidbody2D))]
public class Mover : BaseComponent<IMoveConfig>
{
    //组件是独立的 不应该直接持有玩家管理器 应该持有一个通用的数据包
    //这个数据包由外部管理器依赖注入而来
    private IMoveConfig _moveConfig;
    private float _moveSpeedMultiplier = 1f;//移动的速度乘数
    private Rigidbody2D _rigidbody;
    
    private int _faceRight = 1;
    
    public int FaceRight => _faceRight;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    private void UpdateFace(float horizontalValue)
    {
        if (horizontalValue == 0) return;
        int newFaceRight = horizontalValue > 0 ? 1 : -1;

        //玩家输入方向改变时才执行
        if(newFaceRight != _faceRight)
        {
            _faceRight = newFaceRight;
            transform.localScale = new Vector3(_faceRight, 1, 1);
        }
    }

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        _moveSpeedMultiplier = multiplier;
    }
    public void Move(float horizontalValue)
    {
        if (_moveConfig == null) return;
        
        UpdateFace(horizontalValue);
        //直接写在FixedUpdate里 不管什么状态 都可以移动
        _rigidbody.velocity = new Vector2(
            horizontalValue * _moveConfig.RunSpeed * _moveSpeedMultiplier,
            _rigidbody.velocity.y);
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
    }
    public override bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        return null;
    }
}
}
