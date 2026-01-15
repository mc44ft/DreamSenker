// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.CompilerServices;
// using UnityEngine;
//
// public class PlayerMirrorForm : FormStrategy
// {
//     public override PlayerConfig PlayerConfig { get; }
//     private float m_fireIntervalTimer = 0f;
//
//     public PlayerMirrorForm(PlayerController playerController, MachineManager<PlayerController> fsmManager) : base(playerController, fsmManager)
//     {
//         PlayerConfig = playerController.PlayerConfig.MirrorFormConfig;
//     }
//     public override bool GlobalUpdate()
//     {
//         //全局计时
//         m_fireIntervalTimer -= Time.deltaTime;
//
//         if (base.GlobalUpdate())
//             return true;
//
//         
//         return false;
//     }
//     #region AttackState
//     private IDamageable m_previousSelectedTarget = null;
//     private IDamageable m_currentSelectedTarget = null;
//
//     
//     public override void AttackEnter()
//     {
//         _controller.AttackQuad_Mirror.SetActive(true);
//     }
//
//     public override void AttackExit()
//     {
//         _controller.AttackQuad_Mirror.SetActive(false);
//
//         //取消选中上一个对象
//         if (m_previousSelectedTarget is UnityEngine.Object unityObj && unityObj != null)
//         {
//             m_previousSelectedTarget.Deselect();
//         }
//
//         m_previousSelectedTarget = null;
//         m_currentSelectedTarget = null;
//     }
//
//     public override void AttackLogicUpdate()
//     {
//         _controller.UpdateFace();
//
//         if (InputManager.Instance.AttackButtonUp)
//         {
//             if (m_currentSelectedTarget != null && m_fireIntervalTimer <= 0f)
//             {
//                 AudioManager.Instance.PlaySound(GameResources.Instance.PlayerMirrorFormAttackClip);
//                 //发射子弹
//                 GameObject.Instantiate(
//                     _controller.CurrentFormStrategy.PlayerConfig.AmmoPrefab,
//                     _controller.transform.position, Quaternion.identity).
//                     GetComponent<PlayerAmmo>().
//                     Init(
//                     _controller.CurrentFormStrategy.PlayerConfig.AmmoSpeed,
//                     _controller.CurrentFormStrategy.PlayerConfig.AmmoDamage,
//                     m_currentSelectedTarget.GetPosition());
//
//                 //重置发射间隔计时
//                 m_fireIntervalTimer = _controller.CurrentFormStrategy.PlayerConfig.AmmoFireInterval;
//             }
//
//             _fsmManager.TransitionTo(_controller.PlayerIdleState);
//             return;
//         }
//
//         if(_controller.AttackCheck_Mirror.MonsterList.Count > 0)
//         {
//             //怪物被销毁时 Unity内部重载运算符 这里会等于null 这一步将所有已经死亡的怪物移除列表
//             _controller.AttackCheck_Mirror.MonsterList.RemoveAll(item => item == null);
//             if(_controller.AttackCheck_Mirror.MonsterList.Count > 0)
//             {
//                 
//                 Transform targetTransform = _controller.AttackCheck_Mirror.MonsterList.OrderBy(
//                     target => Vector3.Distance(_controller.transform.position, target.position)).//从小到大排序
//                     FirstOrDefault();//找出最小值
//                 m_currentSelectedTarget = targetTransform.GetComponent<IDamageable>();
//                 //选中该对象
//                 m_currentSelectedTarget?.Select();
//
//                 if (m_currentSelectedTarget != m_previousSelectedTarget)
//                 {
//                     //取消选中上一个对象
//                     //接口指向的是一个引用 他没有被Unity重载 所有即便对象销毁了 他依然保持着这个引用 不会为null
//                     //在这里做一个特殊判断
//                     if (m_previousSelectedTarget is UnityEngine.Object unityObj && unityObj != null)
//                     {
//                         m_previousSelectedTarget.Deselect();
//                     }
//
//                     m_previousSelectedTarget = m_currentSelectedTarget;
//                 }
//                 
//             }
//         }
//     }
//
//     public override void AttackPhysicsUpdate()
//     {
//         //攻击状态不妨碍移动
//         _controller.Rigidbody.velocity = new Vector2(
//             InputManager.Instance.HorizontalValue * PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
//             _controller.Rigidbody.velocity.y);
//     }
//     #endregion
//
//     #region DeathState
//     public override void DeathEnter()
//     {
//         Debug.Log("进入Death");
//         _controller.StartCoroutine(AnimationPlayRoutine());
//     }
//     private IEnumerator AnimationPlayRoutine()
//     {
//         //切换到一形态
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Transition);
//         AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
//         while(info.normalizedTime <= 0.95f)
//         {
//             yield return null;
//         }
//
//         _controller.SwitchForm();
//         yield return new WaitForSeconds(0.5f);
//         
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Death);
//         info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
//         while(info.normalizedTime <= 0.95f)
//         {
//             yield return null;
//         }
//     }
//     public override void DeathExit()
//     {
//         
//     }
//
//     public override void DeathLogicUpdate()
//     {
//         
//     }
//
//     public override void DeathPhysicsUpdate()
//     {
//         
//     }
//     #endregion
//     #region JumpState
//     private float m_flyingTimer;
//     public override void JumpEnter()
//     {
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Jump);
//         _controller.JumpCounter--;
//
//         //设置重力
//         _controller.Rigidbody.gravityScale = _controller.CurrentFormStrategy.PlayerConfig.JumpGravityScale;
//         //设置跳跃速度
//         _controller.Rigidbody.velocity = new Vector2(
//             _controller.Rigidbody.velocity.x, _controller.CurrentFormStrategy.PlayerConfig.JumpSpeed);
//
//         //初始化长跳计时器
//         m_flyingTimer = _controller.CurrentFormStrategy.PlayerConfig.FlyingTime;
//     }
//
//     public override void JumpExit()
//     {
//         
//     }
//
//     public override void JumpLogicUpdate()
//     {
//         //长跳计时
//         if (InputManager.Instance.JumpButtonHold && _controller.JumpCounter <= 0 && m_flyingTimer > 0)
//         {
//             //第一次进入
//             if(m_flyingTimer == _controller.CurrentFormStrategy.PlayerConfig.FlyingTime)
//             {
//                 _controller.StartCoroutine(PlayFlyAnimationRoutine());
//             }
//
//             //消耗飞行时间
//             m_flyingTimer -= Time.deltaTime;
//
//             
//         }
//         //松开按键 立刻停止计时 并停止本次飞行
//         if (InputManager.Instance.JumpButtonUp)
//         {
//             m_flyingTimer = 0;
//         }
//         //更新朝向
//         _controller.UpdateFace();
//     }
//     private IEnumerator PlayFlyAnimationRoutine()
//     {
//         //播放飞行动画
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_FlyEnter);
//         AnimatorStateInfo info = _controller.Animator.GetCurrentAnimatorStateInfo(0);
//         while(info.normalizedTime <= 0.95f)
//             yield return null;
//         _controller.Animator.Play(Settings.PlayerRealAnimNameToHash_Flying);
//     }
//     public override void JumpPhysicsUpdate()
//     {
//         //跳跃时也能控制移动
//         _controller.Rigidbody.velocity = new Vector2(
//             InputManager.Instance.HorizontalValue * _controller.CurrentFormStrategy.PlayerConfig.RunSpeed * _controller.MoveSpeedMultiplier,
//             _controller.Rigidbody.velocity.y);
//
//         if (_controller.Rigidbody.velocity.y < 0)
//         {
//             //切换下落状态
//             _fsmManager.TransitionTo(_controller.PlayerFallState);
//             return;
//         }
//
//         //飞行
//         if (m_flyingTimer < _controller.CurrentFormStrategy.PlayerConfig.FlyingTime && //开始计时了
//             m_flyingTimer > 0f)
//         {
//             //保持逐渐衰减的速度
//             _controller.Rigidbody.velocity = new Vector2(
//             _controller.Rigidbody.velocity.x, 
//             (1.5f + _controller.CurrentFormStrategy.PlayerConfig.FlyingForce) * 
//             m_flyingTimer / _controller.CurrentFormStrategy.PlayerConfig.FlyingTime);
//         }
//     }
//     #endregion
//
// }
