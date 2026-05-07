using Data.ScriptableObjects.Character.Enemy;
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
    //--------------------------------- Data ------------------------------------------
    [SerializeField] private OldManConfigSO _config;
    //--------------------------------- Component ------------------------------------------
    //--------------------------------- Public Parameter ------------------------------------------
    public Health Health => _health;
    public Mover Mover => _mover;
    //--------------------------------- Private Parameter ------------------------------------------
    private Health _health;
    private Mover _mover;
    protected override void Awake()
    {
        base.Awake();
        
        _health = GetComponent<Health>();
        _mover = GetComponent<Mover>();
    }

    public void DoAction(EAction action, string[] parameters)
    {
        
    }
}
