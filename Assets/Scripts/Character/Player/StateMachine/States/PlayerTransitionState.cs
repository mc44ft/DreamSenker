using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTransitionState : StateBase<PlayerController>
{
    public PlayerTransitionState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
    {
    }

    public override void Enter()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.PlayerSwitchFormClip);
        _controller.SwitchForm();
        //播放切换形态的动画
        _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Transition);
    }

    public override void Exit()
    {
        
    }

    public override void LogicUpdate()
    {
        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
        if(info.normalizedTime > 0.95f)
        {
            _machineManager.TransitionTo(_controller.PlayerIdleState);
            return;
        }
    }

    public override void PhysicsUpdate()
    {
        
    }
}
