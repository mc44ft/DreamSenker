using System.Collections;
using UnityEngine;

public class FoxTwoSkillDashState : StateBase<FoxTwoController>
{
    private int _dashCounter;
    private float _dashDurationTimer;
    private Vector2 _startPosition;
    private Vector2 _dashDirectiton;
    public FoxTwoSkillDashState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {

        _controller.StartCoroutine(DashingRoutine());
    }
    private IEnumerator DashingRoutine()
    {
        //如果是隐匿状态 那么先现身
        if (_controller.IsHiding)
        {
            //非连段
            //计算现身位置
            Vector3 showPosition = _controller.FoxBossRoom.GetMinDistanceFromShowPosition(_controller.TargetPlayer.position);
            yield return _controller.StartShow(showPosition);
        }
        //初始化冲刺次数
        _dashCounter = _controller.FoxTwoConfig.SkillDashCount;

        //执行多段冲刺
        while (_dashCounter > 0)
        {
            AudioManager.Instance.PlaySound(GameResources.Instance.FoxTwoSkillDashClip);
            _controller.UpdateFace();
            //计算冲刺方向
            _dashDirectiton = -Vector2.right * _controller.FaceLeft;
            //重置冲刺计时
            _dashDurationTimer = _controller.FoxTwoConfig.SkillDashDuration;
            _startPosition = _controller.Rigidbody.position;
            //播放冲刺动画
            _controller.Animator.Play(Settings.FoxTwoAnimNameToHash_SkillDash, 0, 0);

            //冲刺过程（物理帧循环）
            while (_dashDurationTimer > 0)
            {
                //计算曲线值
                _dashDurationTimer -= Time.fixedDeltaTime;

                float curveValue = _controller.FoxTwoConfig.SkillDashEaseCurve.Evaluate(
                    1 - _dashDurationTimer / _controller.FoxTwoConfig.SkillDashDuration);

                //本质上让Boss追赶曲线上的位置
                //计算本帧的目标位置
                Vector2 targetPos = _startPosition + _dashDirectiton * _controller.FoxTwoConfig.SkillDashDistance * curveValue;

                //这里不用MovePosition 因为MovePosition是一个强制移动到指定位置的方法，可能会卡墙
                //改变速度可以更好的提现物理效果
                Vector2 neededVelocity = (targetPos - _controller.Rigidbody.position) / Time.fixedDeltaTime;
                _controller.Rigidbody.velocity = neededVelocity;

                //等待下一个物理帧
                yield return new WaitForFixedUpdate();
            }
            //冲刺计数
            _dashCounter--;
            yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillDashIntervalTime);

        }

        //多段冲刺结束
        if (_controller.CheckSkillCooldownFinished(_controller.SkillCloneState) &&
                _controller.Health.CurrentHealthAmount <
                _controller.FoxTwoConfig.MaxHealthAmount * _controller.FoxTwoConfig.SkillCloneTriggerHealthPercent)
        {
            //接克隆技能
            _machineManager.TransitionTo(_controller.SkillCloneState);
        }
        else
        {
            //隐匿
            _machineManager.TransitionTo(_controller.HideState);
        }
    }
    public override void Exit()
    {
        _controller.ResetSkillCooldownTimer(_controller.SkillDashState);
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
    }
    
}
