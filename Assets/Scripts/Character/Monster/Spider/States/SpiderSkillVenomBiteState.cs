
using System.Collections;
using UnityEngine;

public class SpiderSkillVenomBiteState : StateBase<SpiderController>
{
    private bool m_isBiteTriggered;
    public SpiderSkillVenomBiteState(SpiderController controller, MachineManager<SpiderController> fsmManager) : base(controller, fsmManager)
    {
    }

    public override void Enter()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.SpiderSkillVenomBiteClip);
        Debug.Log("毒牙攻击");
        _controller.CooldownTimer = 1f;
        //播放攻击动画
        _controller.Animator.Play(Settings.SpiderAnimNameToHash_SkillVenomBite);
    }

    public override void Exit()
    {
        m_isBiteTriggered = false;
        //重置计时器
        _controller.SkillVenomBiteCooldownTimer = _controller.SpiderConfig.SkillVenomBiteCooldown;
    }

    public override void LogicUpdate()
    {
        AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
        if(info.normalizedTime > 0.85f && !m_isBiteTriggered)
        {
            Collider2D hit = Physics2D.OverlapBox(
                _controller.SkillVenomBiteZone.transform.position, 
                _controller.SkillVenomBiteZone.bounds.size, 
                0f,
                1 << LayerMask.NameToLayer("PlayerReal"));
            if(hit != null)
            {
                
                hit.GetComponent<IDamageable>().TakeDamage(
                    _controller.SpiderConfig.SkillVenomBiteDamage, 
                    new Vector2(_controller.FaceLeft, 0));

                _controller.StartCoroutine(VenomBiteRoutine(hit.GetComponent<PlayerController>()));
            }


            m_isBiteTriggered = true;
        }
        if (_controller.CooldownTimer < 0f)
        {
            _machineManager.TransitionTo(_controller.SpiderIdleState);
            return;
        }
        _controller.CooldownTimer -= Time.deltaTime;
    }

    public override void PhysicsUpdate()
    {
        
    }
    private IEnumerator VenomBiteRoutine(PlayerController player)
    {
        float timer = _controller.SpiderConfig.SkillVenomBiteContinueTime;
        float intervalTimer = _controller.SpiderConfig.SkillVenomBiteContinueDamageIntervalTime;
        while(timer > 0f)
        {
            if(intervalTimer < 0f)
            {
                player.Health.ApplyDamage(_controller.SpiderConfig.SkillVenomBiteContinueDamage);
                _controller.StartCoroutine(SpriteColorRoutine(player.SpriteRenderer, _controller.SpiderConfig.SkillVenomBiteContinueDamageIntervalTime / 2));

                //恢复计时项
                intervalTimer = _controller.SpiderConfig.SkillVenomBiteContinueDamageIntervalTime;
            }

            //计时
            intervalTimer -= Time.deltaTime;
            timer -= Time.deltaTime;
            yield return null;
        }
    }
    private IEnumerator SpriteColorRoutine(SpriteRenderer spriteRenderer, float duration)
    {
        Debug.Log("触发毒伤");
        spriteRenderer.color = new Color32(0, 69, 4, 255);
        yield return new WaitForSeconds(duration);
        spriteRenderer.color = Color.white;
    }
}
