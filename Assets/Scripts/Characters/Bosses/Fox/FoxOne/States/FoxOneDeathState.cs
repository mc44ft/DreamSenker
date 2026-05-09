using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamSenker.Characters.Bosses
{
public class FoxOneDeathState : StateBase<FoxOneController>
{
    public FoxOneDeathState(FoxOneController controller, MachineManager<FoxOneController> machineManager) : base(controller, machineManager)
    {
    }

    public override void Enter()
    {
        _controller.StartCoroutine(DeathRoutine());

    }

    private IEnumerator DeathRoutine()
    {
        _controller.StopFloating();
        yield return  _controller.StartHide();
        _controller.PushSelfToPool();
    }
    public override void Exit()
    {
        
    }

    public override void LogicUpdate()
    {
        
    }

    public override void PhysicsUpdate()
    {
        
    }
}
}
