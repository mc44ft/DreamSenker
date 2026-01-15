// using UnityEngine;
//
// public class PlayerIdleState : StateBase<PlayerController>
// {
//     public PlayerIdleState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//
//     public override void Enter()
//     {
//         //播放待机动画
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Idle);
//         //强制速度归零
//         _controller.Rigidbody.velocity = Vector2.zero;
//     }
//     public override void Exit()
//     {
//
//     }
//     public override void LogicUpdate()
//     {
//         //仅在待机状态下可以切换形态
//         if (InputManager.Instance.SwitchFormButtonDown && _controller.PlayerSaveData.IsUnlockSwitchStateSkill)
//         {
//             //切换形态
//             _machineManager.TransitionTo(_controller.PlayerTransitionState);
//             return;
//         }
//     }
//     public override void PhysicsUpdate()
//     {
//
//     }
// }