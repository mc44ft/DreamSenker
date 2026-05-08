using Data.ScriptableObjects.Character.Enemy;
using PlayArk.StateMachine;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent((typeof(Mover)))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent((typeof(AnimationPlayer)))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(DamageableHealth))]
[RequireComponent(typeof(OldManBrain))]
[RequireComponent(typeof(OldManActionDriver))]
public class OldManController : StateMachineController
{
    //--------------------------------- Data ------------------------------------------
    [SerializeField] private OldManConfigSO _config;
    //--------------------------------- Component ------------------------------------------
    private DamageableHealth _damageableHealth;
    private Mover _mover;
    private Attacker _attacker;
    private OldManBrain _brain;

    //--------------------------------- Public Parameter ------------------------------------------
    public DamageableHealth DamageableHealth => _damageableHealth;
    public Mover Mover => _mover;

    protected override void Awake()
    {
        base.Awake();

        // 缓存组件
        _damageableHealth = GetComponent<DamageableHealth>();
        _mover = GetComponent<Mover>();
        _attacker = GetComponent<Attacker>();
        _brain = GetComponent<OldManBrain>();

        // 注入配置
        if (_config != null)
        {
            var config = _config.Config;
            _mover?.InjectionConfig(config);
            _attacker?.InjectionConfig(config);
            _brain?.InjectionConfig(config);
            _damageableHealth?.InjectionConfig(config);
        }
    }
}
