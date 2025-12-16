using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SpiderSkillShakeState : StateBase<SpiderController>
{
    public SpiderSkillShakeState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {

        _controller.StartCoroutine(ShakeRoutine());
    }

    public override void Exit()
    {
        //重置计时器
        _controller.SkillhakeCooldownTimer = _controller.SpiderConfig.SkillShakeCooldown;
    }
    private IEnumerator ShakeRoutine()
    {
        float[] intervalTimeArray = _controller.SpiderConfig.WebHitIntervalTimeArray;
        float[] cameraShakeForceArray = _controller.SpiderConfig.WebHitCameraShakeForceArray;

        //等待技能前摇
        yield return new WaitForSeconds(_controller.SpiderConfig.SkillShakeWindUpTime);

        for (int i = 0; i < cameraShakeForceArray.Length; i++)
        {
            AudioManager.Instance.PlaySound(GameResources.Instance.SpiderSkillShakeClip);
            //参数二 动画层级
            //参数三 开始播放的时间
            _controller.Animator.Play(Settings.SpiderAnimNameToHash_SkillShake, 0, 0);

            while (true)
            {
                AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
                if (info.normalizedTime > 0.60f)
                {
                    //脚跺下去之后才触发震动 和 范围 伤害
                    GameManager.Instance.CameraShake(cameraShakeForceArray[i]);
                    break;
                }
                yield return null;
            }
            //在这段时间内将伤害扩散开
            float intervalTime = intervalTimeArray[i];

            _controller.RedWebMask.UpdateDamage(_controller.SpiderConfig.WebHitDamageArray[i]);

            _controller.RedWebMask.transform.localScale = Vector3.zero;

            _controller.RedWebMask.transform.DOScale(60, intervalTime * 0.8f).
                SetEase(Ease.OutExpo).
                OnComplete(() =>
                {
                    _controller.RedWebMask.transform.DOScale(0, intervalTime * 0.2f);
                });

            yield return new WaitForSeconds(intervalTime);
        }

        _machineManager.TransitionTo(_controller.SpiderIdleState);
    }
    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
}
