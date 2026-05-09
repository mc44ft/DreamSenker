using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using DreamSenker.Shared;

namespace DreamSenker.Characters.Bosses
{
public class FoxTwoDeathState : StateBase<FoxTwoController>
{
    public FoxTwoDeathState(FoxTwoController controller, MachineManager<FoxTwoController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        Debug.Log("Boss死亡");
        _controller.StartCoroutine(DeathRoutine());
    }
    private IEnumerator DeathRoutine()
    {
        _controller.Rigidbody.velocity = Vector3.zero;
        //强制显示
        _controller.Material.DOFloat(1f, Settings.DissolveAmountString, 0.1f).
                SetEase(Ease.InOutQuad);
        //消亡
        yield return _controller.StartHide(true, 4);

        yield return new WaitForSeconds(_controller.FoxTwoConfig.DeathIntervalTime);

        //宣告死亡
        EventCenter.Instance.EventTrigger(
                E_EventType.Game_BossDead,
                this,
                new GameBossDeadEventArgs(EBossType.FoxTwo, _controller.gameObject));

    }
    public override void LogicUpdate()
    {
    }

    public override void PhysicsUpdate()
    {
    }

    public override void Exit()
    {
    }
}
}
