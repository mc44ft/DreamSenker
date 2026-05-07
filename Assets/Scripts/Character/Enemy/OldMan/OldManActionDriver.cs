using PlayArk.StateMachine.Utilities;
using UnityEngine;

/// <summary>
/// OldMan的动作执行组件
/// 负责接收状态机EAction并调用功能组件
/// </summary>
public class OldManActionDriver : MonoBehaviour, IAction
{
    private OldManBrain _brain;
    private Mover _mover;
    private AnimationPlayer _animationPlayer;

    private void Awake()
    {
        _brain = GetComponent<OldManBrain>();
        _mover = GetComponent<Mover>();
        _animationPlayer = GetComponent<AnimationPlayer>();
    }

    public void DoAction(EAction action, string[] parameters)
    {
        switch (action)
        {
            case EAction.Move:
                _mover?.Move(_brain.MoveDirection);
                break;

            case EAction.StopMove:
                _mover?.StopMove();
                break;

            case EAction.PlayAnimation:
                if (parameters != null && parameters.Length > 0)
                {
                    _animationPlayer?.PlayAnimation(parameters[0]);
                }
                break;

            case EAction.Attack:
                // 触发攻击动画，攻击冷却由Attacker内部维护
                _animationPlayer?.PlayAnimation("Attack");
                break;
        }
    }
}
