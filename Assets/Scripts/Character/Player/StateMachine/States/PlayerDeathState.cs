using UnityEngine;

public class PlayerDeathState : StateBase<PlayerController>
{

    public PlayerDeathState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
    {
    }

    public override void Enter()
    {
        _controller.CurrentFormStrategy.DeathEnter();

    }

    public override void Exit()
    {
        _controller.CurrentFormStrategy.DeathExit();
    }

    public override void LogicUpdate()
    {
        _controller.CurrentFormStrategy.DeathLogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        _controller.CurrentFormStrategy.DeathPhysicsUpdate();
    }
}
