using DG.Tweening;
using System.Collections;
using UnityEngine;

using DreamSenker.Combat.Health;
using DreamSenker.Shared;

namespace DreamSenker.Characters.Bosses
{
public class FoxTwoSkillJumpState : StateBase<FoxTwoController>
{
    //维护一个受击列表
    //进行多帧的攻击判断
    //当列表中有该玩家时 跳过不检测
    //因为只有一个玩家，所以这里不用列表，用一个引用即可
    private Collider2D _hitCheckTarget;
    public FoxTwoSkillJumpState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        _controller.StartCoroutine(jumpingRoutine());
    }
    private IEnumerator jumpingRoutine()
    {
        //计算现身点
        Vector3 showPosition = _controller.FoxBossRoom.GetMaxDistanceFromShowPosition(_controller.TargetPlayer.position);
        //等待现身结束
        yield return _controller.StartShow(showPosition);
        //播放跳跃动画
        _controller.Animator.Play(Settings.FoxTwoAnimNameToHash_SkillJump, 0, 0);

        //计算落点
        Vector3 landingPosition = new Vector3(_controller.TargetPlayer.position.x, _controller.FoxBossRoom.GetGroundY(), 0f);
        //计算最高点高度
        float peakY = _controller.FoxBossRoom.GetGroundY() + _controller.FoxTwoConfig.SkillJumpPower;
        //计算跳跃过程的总时间
        float totalTime = _controller.FoxTwoConfig.SkillJumpDuration +
            _controller.FoxTwoConfig.SkillJumpToFallIntervalTime +
            _controller.FoxTwoConfig.SkillJumpFallDuration;

        Sequence sequence = DOTween.Sequence().SetLink(_controller.gameObject);
        //时间轴模式执行序列
        //横纵轴一起移动
        sequence.Insert(0, _controller.transform.DOMoveX((showPosition.x + landingPosition.x) / 2, _controller.FoxTwoConfig.SkillJumpDuration)).
            SetEase(Ease.Linear);//X轴匀速
        sequence.Insert(0, _controller.transform.DOMoveY(peakY, _controller.FoxTwoConfig.SkillJumpDuration)).
            SetEase(_controller.FoxTwoConfig.SkillJumpEaseCurve);

        //下落
        //等两段时间执行
        sequence.Insert(_controller.FoxTwoConfig.SkillJumpDuration + _controller.FoxTwoConfig.SkillJumpToFallIntervalTime,
            _controller.transform.DOMoveX(landingPosition.x, _controller.FoxTwoConfig.SkillJumpFallDuration).
            SetEase(Ease.Linear));//X轴匀速

        sequence.Insert(_controller.FoxTwoConfig.SkillJumpDuration + _controller.FoxTwoConfig.SkillJumpToFallIntervalTime,
            _controller.transform.DOMoveY(landingPosition.y, _controller.FoxTwoConfig.SkillJumpFallDuration).
            SetEase(_controller.FoxTwoConfig.SkillJumpFallEaseCurve));

        //等待序列执行结束
        yield return sequence.WaitForCompletion();


        //下落结束 开始抓挠动作
        //播放动画
        _controller.Animator.Play((Settings.FoxTwoAnimNameToHash_SkillJumpScratck));
        //进行伤害判断前先置空检测对象
        _hitCheckTarget = null;

        //等待切换动画
        yield return null;
        //等待动画
        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime < 0.3f)
        {
            info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        //伤害判断
        while (info.normalizedTime < 0.95f)
        {
            Collider2D targetCollider =  Physics2D.OverlapBox(
                _controller.ScratchCheckCollider2D.bounds.center,  
                _controller.ScratchCheckCollider2D.bounds.size, 
                0f, 
                _controller.FoxTwoConfig.AttackLayerMask);
            if (targetCollider != null && targetCollider != _hitCheckTarget)
            {
                if(targetCollider.TryGetComponent<IDamageable>(out IDamageable damageable))
                {
                    damageable.TakeDamage(_controller.FoxTwoConfig.SkillJumpScratchDamage, 
                        new Vector2(-_controller.FaceLeft, 0));
                    //记录对象 不进行第二次攻击
                    _hitCheckTarget = targetCollider;
                }
            }
            info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        //动画播放完毕
        //呆滞时间
        yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillJumpScratchIntervalTime);
        //检查Dash是否准备好 （3+2连段）
        if (_controller.CheckSkillCooldownFinished(_controller.SkillDashState))
        {
            _machineManager.TransitionTo(_controller.SkillDashState);
        }
        else
        {
            _machineManager.TransitionTo(_controller.HideState);
        }
    }

    public override void Exit()
    {
        //重置技能CD
        _controller.ResetSkillCooldownTimer(_controller.SkillJumpState);
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
    
}
}
