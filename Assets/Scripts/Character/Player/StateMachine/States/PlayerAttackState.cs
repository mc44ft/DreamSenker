// using UnityEngine;
//
// public class PlayerAttackState : StateBase<PlayerController>
// {
//     
//     public PlayerAttackState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//
//     public override void Enter()
//     {
//         _controller.CurrentFormStrategy.AttackEnter();
//     }
//
//     public override void Exit()
//     {
//         _controller.CurrentFormStrategy.AttackExit();
//     }
//
//     public override void LogicUpdate()
//     {
//         _controller.CurrentFormStrategy.AttackLogicUpdate();
//     }
//
//     public override void PhysicsUpdate()
//     {
//         _controller.CurrentFormStrategy.AttackPhysicsUpdate();
//     }
// }
