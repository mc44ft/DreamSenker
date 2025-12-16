using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderSkillHealState : StateBase<SpiderController>
{
    public SpiderSkillHealState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {
        //播放回血动画
        _controller.Animator.Play(Settings.SpiderAnimNameToHash_SkillHeal);
        //放置Egg
        _controller.EggGameObject.SetActive(true);

        //回血
        _controller.Health.ApplyDamage(-_controller.SpiderConfig.HealAmount);

        //重置计时器
        _controller.CooldownTimer = _controller.SpiderConfig.HealWindupTime;
    }

    public override void Exit()
    {
        _controller.EggGameObject.SetActive(false);
        //重置计时器
        _controller.SkillHealCooldownTimer = _controller.SpiderConfig.SkillHealCooldown;
    }

    public override void LogicUpdate()
    {
        if (_controller.CooldownTimer < 0f)
        {
            _controller.HealthAmount += _controller.SpiderConfig.HealAmount;
            _machineManager.TransitionTo(_controller.SpiderIdleState);
            return;
        }
        _controller.CooldownTimer -= Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        
    }
}
