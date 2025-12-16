using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRealForm : FormStrategy
{
    public bool IsAttacked {  get; private set; }

    public override PlayerConfig PlayerConfig { get; }

    public PlayerRealForm(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
    {
        PlayerConfig = playerController.PlayerConfig.RealFormConfig;
    }
    public override bool GlobalUpdate()
    {
        if (base.GlobalUpdate())
            return true;

        return false;
            
    }
    #region AttackState
    public override void AttackEnter()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.PlayerRealFormAttackClip);

        //播放攻击动画
        _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Attack);
    }

    public override void AttackExit()
    {
        IsAttacked = false;
    }

    public override void AttackLogicUpdate()
    {
        //攻击时可以转向
        _controller.UpdateFace();

        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);

        if (info.shortNameHash != Settings.PlayerRealAnimNameToHash_Attack) return;

        if (info.normalizedTime >= 0.5f && !IsAttacked)
        {
            //攻击检测
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                _controller.AttackCheckCollider_Real.transform.position,
                _controller.AttackCheckCollider_Real.bounds.size,
                0f,
                PlayerConfig.AttackCheckLayer);
            if (hits.Length > 0)
            {
                foreach (Collider2D hit in hits)
                {
                    if(hit.TryGetComponent(out IDamageable damageable))
                    {
                        //Buff脏代码
                        if (_controller.IsDebuff && Random.value < 0.4f)
                        {
                            hit.gameObject.AddComponent<MournfulGazeDebuff>();
                        }


                        damageable.TakeDamage(
                        (int)(PlayerConfig.AttackDamage * _controller.PlayerSaveData.AttackMultiply),
                        new Vector2(_controller.FaceRight, 0));
                    }
                }
            }
            IsAttacked = true;

        }

        if (info.normalizedTime >= 0.95f)
        {
            if (!_controller.IsGround)
            {
                //切换下落状态
                _fsmManager.TransitionTo(_controller.PlayerFallState);
                return;
            }
            else
            {
                if (InputManager.Instance.HorizontalValue != 0)
                {
                    _fsmManager.TransitionTo(_controller.PlayerRunState);
                    return;
                }
                else
                {
                    _fsmManager.TransitionTo(_controller.PlayerIdleState);
                    return;
                }
            }
        }
    }

    public override void AttackPhysicsUpdate()
    {
        //攻击状态不妨碍移动
        _controller.Rigidbody.velocity = new Vector2(
            InputManager.Instance.HorizontalValue * PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
            _controller.Rigidbody.velocity.y);
    }
    #endregion

    #region DeathState
    public override void DeathEnter()
    {
        _controller.Rigidbody.bodyType = RigidbodyType2D.Static;
        //播放死亡动画
        _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Death);
    }

    public override void DeathExit()
    {
        
    }

    public override void DeathLogicUpdate()
    {
        
    }

    public override void DeathPhysicsUpdate()
    {
        
    }
    #endregion

    #region JumpState
    public override void JumpEnter()
    {
        //播放Jump动画
        _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Jump);

        //跳跃次数减一
        _controller.JumpCounter--;

        //设置重力
        _controller.Rigidbody.gravityScale = _controller.CurrentFormStrategy.PlayerConfig.JumpGravityScale;

        //设置跳跃速度
        _controller.Rigidbody.velocity = new Vector2(
            _controller.Rigidbody.velocity.x, _controller.CurrentFormStrategy.PlayerConfig.JumpSpeed);

        
    }

    public override void JumpExit()
    {
        
    }

    public override void JumpLogicUpdate()
    {
        //更新朝向
        _controller.UpdateFace();
    }

    public override void JumpPhysicsUpdate()
    {
        //跳跃时也能控制移动
        _controller.Rigidbody.velocity = new Vector2(
            InputManager.Instance.HorizontalValue * _controller.CurrentFormStrategy.PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
            _controller.Rigidbody.velocity.y);

        if (_controller.Rigidbody.velocity.y < 0)
        {
            //切换下落状态
            _fsmManager.TransitionTo(_controller.PlayerFallState);
            return;
        }
    }
    #endregion
}
