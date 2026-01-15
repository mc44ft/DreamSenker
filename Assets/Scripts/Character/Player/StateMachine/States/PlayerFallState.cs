// using UnityEngine;
//
// public class PlayerFallState : StateBase<PlayerController>
// {
//     public PlayerFallState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//     public override void Enter()
//     {
//         //播放下落动画
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Fall);
//         //设置重力
//         _controller.Rigidbody.gravityScale = _controller.CurrentFormStrategy.PlayerConfig.FallGravityScale;
//     }
//
//     public override void Exit()
//     {
//
//     }
//
//     public override void LogicUpdate()
//     {
//         if (_controller.IsGround)
//         {
//             if (InputManager.Instance.HorizontalValue != 0)
//             {
//                 _machineManager.TransitionTo(_controller.PlayerRunState);
//                 return;
//             }
//             else
//             {
//                 //切换到待机状态
//                 _machineManager.TransitionTo(_controller.PlayerIdleState);
//                 return;
//             }
//         }
//         //更新朝向
//         _controller.UpdateFace();
//     }
//
//     public override void PhysicsUpdate()
//     {
//         //下落时也能控制移动
//         _controller.Rigidbody.velocity = new Vector2(
//             InputManager.Instance.HorizontalValue * _controller.CurrentFormStrategy.PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
//             _controller.Rigidbody.velocity.y);
//     }
// }