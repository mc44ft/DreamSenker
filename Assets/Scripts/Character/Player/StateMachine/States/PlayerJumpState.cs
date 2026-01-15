// using UnityEngine;
//
// public class PlayerJumpState : StateBase<PlayerController>
// {
//     public PlayerJumpState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//     public override void Enter()
//     {
//         _controller.CurrentFormStrategy.JumpEnter();
//     }
//
//     public override void Exit()
//     {
//         _controller.CurrentFormStrategy.JumpExit();
//     }
//
//     public override void LogicUpdate()
//     {
//         _controller.CurrentFormStrategy.JumpLogicUpdate();
//     }
//
//     public override void PhysicsUpdate()
//     {
//         _controller.CurrentFormStrategy.JumpPhysicsUpdate();
//     }
// }
