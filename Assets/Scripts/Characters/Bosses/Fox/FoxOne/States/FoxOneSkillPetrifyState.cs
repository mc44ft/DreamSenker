using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using DreamSenker.Shared;
using DreamSenker.Traps.Buffs;

namespace DreamSenker.Characters.Bosses
{
public class FoxOneSkillPetrifyState : StateBase<FoxTwoController>
{
    private bool _isAddedBuff;
    public FoxOneSkillPetrifyState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        _controller.StartCoroutine(PetrifyRoutine());
    }
    private IEnumerator PetrifyRoutine()
    {
        _controller.Rigidbody.velocity = Vector3.zero;  

        //计算现身点
        Vector3 showPosition = _controller.FoxBossRoom.GetMinDistanceFromShowPosition(_controller.TargetPlayer.position);
        //等待现身结束
        yield return _controller.StartShow(showPosition);

        _controller.Animator.Play(Settings.FoxTwoAnimNameToHash_SkillPetrify, 0, 0);
        //等一帧切换动画
        yield return null;
        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);

        //重置标识
        _isAddedBuff = false;

        //等待动画结束
        while (info.normalizedTime < 0.95f)
        {
            if(_controller.PetrifyCheck.Target != null && !_isAddedBuff)
            {
                if (_controller.PetrifyCheck.Target.TryGetComponent(out PetrifyDebuff debuff))
                {
                    //玩家身上还有buff 重置buff效果
                    debuff.Initialize(_controller.FoxTwoConfig.SkillPetrifyTime);
                }
                else
                {
                    //玩家身上没有buff 添加buff
                    _controller.PetrifyCheck.Target.AddComponent<PetrifyDebuff>().Initialize(_controller.FoxTwoConfig.SkillPetrifyTime);
                }
                _isAddedBuff = true;
            }
            info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        //石化释放后的呆滞时间
        yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillPetrifyedTime);

        _machineManager.TransitionTo(_controller.HideState);
    }
    public override void Exit()
    {
        _controller.ResetSkillCooldownTimer(_controller.SkillPetrifyState);
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
}
}
