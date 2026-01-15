// using UnityEngine;
//
// /// <summary>
// /// 策略模式
// /// 根据不同的形态转换策略
// /// </summary>
// public abstract class FormStrategy
// {
//     public abstract PlayerConfig PlayerConfig {  get; }
//
//     protected PlayerController _controller;
//     protected MachineManager<PlayerController> _fsmManager;
//     public FormStrategy(PlayerController playerController, MachineManager<PlayerController> fsmManager)
//     {
//         _controller = playerController;
//         _fsmManager = fsmManager;
//     }
//     //全域拦截
//     /// <summary>
//     /// 返回值标示了父类是否进行了拦截 如果父类已经拦截 那么子类不需要再次拦截
//     /// </summary>
//     /// <returns></returns>
//     public virtual bool GlobalUpdate()
//     {
//         //死亡
//         if (_controller.Health.IsDead && _fsmManager.CurrentState != _controller.PlayerDeathState)
//         {
//             _fsmManager.TransitionTo(_controller.PlayerDeathState);
//             return true;
//         }
//         //过渡状态不处理之后的任何拦截
//         else if (_fsmManager.CurrentState == _controller.PlayerTransitionState)
//         {
//             return true;
//         }
//         //除了死亡，过渡状态和受击状态 任何状态都可受击
//         else if (!_controller.Health.IsDead &&
//             _controller.IsGetHit &&
//             _fsmManager.CurrentState != _controller.PlayerGetHitState)
//         {
//             _fsmManager.TransitionTo(_controller.PlayerGetHitState);
//             return true;
//         }
//         //除了死亡，受击,只要在地面上 按下移动键一定是移动
//         //放在攻击的切换后面，在玩家按住移动键的同时按下攻击键时，保证先进入AttackEnter播放动画后下一帧再切回Run状态
//         else if (!_controller.Health.IsDead && _controller.IsGround && InputManager.Instance.HorizontalValue != 0
//             && _fsmManager.CurrentState != _controller.PlayerRunState &&
//             _fsmManager.CurrentState != _controller.PlayerAttackState && //攻击状态也具有移动的能力 等其自然转换
//             _fsmManager.CurrentState != _controller.PlayerGetHitState)
//         {
//             _fsmManager.TransitionTo(_controller.PlayerRunState);
//             return true;
//         }
//         //除了死亡，任何状态都可跳跃
//         if (!_controller.Health.IsDead && InputManager.Instance.JumpButtonDown && _controller.JumpCounter > 0)
//         {
//             _fsmManager.TransitionTo(_controller.PlayerJumpState);
//             return true;
//         }
//         //除了死亡，和攻击状态 技能状态 受击状态 任何状态都可攻击
//         else if (!_controller.Health.IsDead && InputManager.Instance.AttackButtonDown &&
//             _fsmManager.CurrentState != _controller.PlayerAttackState &&
//             _fsmManager.CurrentState != _controller.PlayerGetHitState)
//         {
//             _fsmManager.TransitionTo(_controller.PlayerAttackState);
//             return true;
//         }
//         return false;
//     }
//     //形态不同逻辑不同的状态
//     //Attack
//     public abstract void AttackEnter();
//     public abstract void AttackExit();
//     public abstract void AttackLogicUpdate();
//     public abstract void AttackPhysicsUpdate();
//     //Death
//     public abstract void DeathEnter();
//     public abstract void DeathExit();
//     public abstract void DeathLogicUpdate();
//     public abstract void DeathPhysicsUpdate();
//
//     //Jump
//     public abstract void JumpEnter();
//     public abstract void JumpExit();
//     public abstract void JumpLogicUpdate();
//     public abstract void JumpPhysicsUpdate();
// }
