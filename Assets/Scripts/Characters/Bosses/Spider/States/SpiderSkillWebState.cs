
using UnityEngine;

using DreamSenker.Data.Configs.Character.Monster;
using DreamSenker.Managers;

namespace DreamSenker.Characters.Bosses
{
public class SpiderSkillWebState : StateBase<SpiderController>
{
    public SpiderSkillWebState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.SpiderSkillWebClip);
        //重置计时器
        _controller.CooldownTimer = _controller.SpiderConfig.SilkWindupTime;
    }

    public override void Exit()
    {
        //重置计时器
        _controller.SkillWebCooldownTimer = _controller.SpiderConfig.SkillWebCooldown;
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        //在Update中创建蛛丝 但是蛛丝在FixedUpdate中移动，会造成闪烁 所以这里将创建蛛丝的逻辑放在FixedUpdate中执行
        if (_controller.CooldownTimer < 0f)
        {
            SpiderConfigSO config = _controller.SpiderConfig;

            //生成蛛丝
            SpiderSilk silk = GameObject.Instantiate(_controller.SpiderConfig.SpiderSilkPrefab, _controller.transform.position, Quaternion.identity).GetComponent<SpiderSilk>();
            silk.Init(_controller.Player.position, 
                config.SilkContinueTime, config.SilkSpeed, config.SilkClampSpeedRate, config.SilkHealthAmount);

            _machineManager.TransitionTo(_controller.SpiderIdleState);
            return;
        }
        _controller.CooldownTimer -= Time.deltaTime;
    }
}
}
