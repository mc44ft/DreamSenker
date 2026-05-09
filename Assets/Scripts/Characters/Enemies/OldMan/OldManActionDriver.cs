using PlayArk.StateMachine.Utilities;
using UnityEngine;


using DreamSenker.Characters.Player;

/// <summary>
/// OldMan的动作执行组件
/// 负责接收状态机EAction并调用功能组件
/// </summary>

namespace DreamSenker.Characters.Enemies
{
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
        }
    }
}
}
