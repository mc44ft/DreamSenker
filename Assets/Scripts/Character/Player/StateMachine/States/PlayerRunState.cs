// using UnityEngine;
//
// public class PlayerRunState : StateBase<PlayerController>
// {
//     public PlayerRunState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//
//     public override void Enter()
//     {
//         AudioManager.Instance.PlaySound(GameResources.Instance.PlayerRunClip, false);
//         //播放Run动画
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Run);
//     }
//
//     public override void Exit()
//     {
//         AudioManager.Instance.StopSound();
//     }
//
//     public override void LogicUpdate()
//     {
//         if (InputManager.Instance.HorizontalValue == 0f)
//         {
//             //切换Idle状态
//             _machineManager.TransitionTo(_controller.PlayerIdleState);
//             return;
//         }
//         else if (!_controller.IsGround)//不处于接地状态 唯一的可能就是下落了
//         {
//             _machineManager.TransitionTo(_controller.PlayerFallState);
//             return;
//         }
//         //更新朝向
//         //在最后更新朝向 当horizontal为0时 不会触发更新朝向的方法
//         //玩家会保持最后一次移动的朝向
//         _controller.UpdateFace();
//     }
//
//     public override void PhysicsUpdate()
//     {
//         _controller.Rigidbody.velocity = new Vector2(
//             InputManager.Instance.HorizontalValue * _controller.CurrentFormStrategy.PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
//             _controller.Rigidbody.velocity.y);
//     }
// }
