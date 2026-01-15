// using UnityEngine;
// /// <summary>
// /// 玩家受击时应该有短暂的无敌帧 这时候不接受任何攻击 这里似乎还没做
// /// </summary>
// public class PlayerGetHitState : StateBase<PlayerController>
// {
//     private bool m_isTriggeredHitStop = false;
//     public PlayerGetHitState(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//     }
//
//     public override void Enter()
//     {
//         //播放受伤动画
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Gethit);
//
//         //Debug
//         //反方向弹飞玩家
//         //这里优化成斜上角
//         HelperUtilities.DoKnockback(_controller.Rigidbody, _controller.GethitDirection,
//             _controller.CurrentFormStrategy.PlayerConfig.GetHitKnockbackForceValue);
//     }
//
//     public override void Exit()
//     {
//         _controller.IsGetHit = false;
//         m_isTriggeredHitStop = false;
//     }
//
//     public override void LogicUpdate()
//     {
//         AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
//
//         //Unity的Animator状态更新是在下一帧 所以这里加一个判断 
//         //如果不加判断 这里得到的info就是上一个状态已经循环多次的info
//         if (info.shortNameHash != Settings.PlayerRealAnimNameToHash_Gethit) return;
//
//
//         if (info.normalizedTime >= 0.5f && !m_isTriggeredHitStop)
//         {
//             //相机震动
//             GameManager.Instance.CameraShake();
//             //顿帧
//             GameManager.Instance.DoHitStop(_controller.CurrentFormStrategy.PlayerConfig.GetHitStopTime);
//
//             m_isTriggeredHitStop = true;
//         }
//         if (info.normalizedTime >= 0.95f)
//         {
//             if (_controller.IsGround)
//             {
//                 _machineManager.TransitionTo(_controller.PlayerIdleState);
//                 return;
//             }
//             else
//             {
//                 _machineManager.TransitionTo(_controller.PlayerFallState);
//                 return;
//             }
//         }
//     }
//
//     public override void PhysicsUpdate()
//     {
//
//     }
// }
