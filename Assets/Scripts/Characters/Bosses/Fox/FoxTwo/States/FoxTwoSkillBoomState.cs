using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
public class FoxTwoSkillBoomState : StateBase<FoxTwoController>
{
    private List<FoxBoomFlame> _boomFireList = new List<FoxBoomFlame>();
    public FoxTwoSkillBoomState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        _controller.StartCoroutine(BoomRoutine());
    }
    private IEnumerator BoomRoutine()
    {
        _controller.Rigidbody.velocity = Vector2.zero;
        //计算现身点
        Vector3 showPosition = _controller.FoxBossRoom.GetMaxDistanceFromShowPosition(_controller.TargetPlayer.position);
        //等待现身结束
        yield return _controller.StartShow(showPosition);

        //执行动画播放协程
        _controller.StartCoroutine(BoomAnimationRoutine());

        for (int i = 0; i < _controller.FoxTwoConfig.SkillBoomFireCount; i++)
        {
            FoxBoomFlame boomFire = PoolManager.Instance.Pull<FoxBoomFlame>(_controller.FoxTwoConfig.SkillBoomFirePrefab);

            //将火球位置设置在尾巴上
            boomFire.ResetTransformInfo(_controller.BoomFirePoint.position, Quaternion.identity);
            boomFire.SetParent(_controller.BoomFirePoint, true);

            _boomFireList.Add(boomFire);
        }

        //蓄力
        TimerManager.Countdown(_controller.FoxTwoConfig.SkillBoomFireLaunchChargeTime, () =>
        {
            foreach (FoxBoomFlame boomFire in _boomFireList)
            {
                //计算随机落点
                Vector3 endPosition = new Vector3(
                    Random.Range(_controller.TargetPlayer.position.x - _controller.FoxTwoConfig.SkillBoomFireLaunchAccuracyOffset,
                    _controller.TargetPlayer.position.x + _controller.FoxTwoConfig.SkillBoomFireLaunchAccuracyOffset),
                    _controller.FoxBossRoom.GetGroundY(), 0);

                //发射火球
                boomFire.Launch(endPosition, _controller.FoxTwoConfig.SkillBoomFireLaunchPower, _controller.FoxTwoConfig.SkillBoomFireLaunchDuration);
                boomFire.SetParent(null, true);
            }
            _boomFireList.Clear();
        });

        yield return new WaitForSeconds(_controller.FoxTwoConfig.SkillBoomLaunchedTime);

        _machineManager.TransitionTo(_controller.HideState);
    }
    private IEnumerator BoomAnimationRoutine()
    {
        _controller.Animator.Play(Settings.FoxTwoAnimNameToHash_SkillBoom);
        //Animator下一帧才会切换状态
        yield return null;
        //获取动画信息
        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
        //等待动画的长度
        yield return new WaitForSeconds(info.length);

        //播放循环动画
        _controller.Animator.Play(Settings.FoxTwoAnimNameToHash_SkillBoomLoop);
    }
    
    public override void Exit()
    {
        _boomFireList.Clear();
        _controller.ResetSkillCooldownTimer(_controller.SkillBoomState);
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
}
}
