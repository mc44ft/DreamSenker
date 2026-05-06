using PlayArk.StateMachine;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
[RequireComponent(typeof(Animator))]
[RequireComponent((typeof(Mover)))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent((typeof(AnimationPlayer)))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Health))]
public class OldManController : StateMachineController, IAction
{
    public void DoAction(EAction action, string[] parameters)
    {
        
    }
}
