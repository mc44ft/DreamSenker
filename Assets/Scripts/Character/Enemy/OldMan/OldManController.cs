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
[RequireComponent(typeof(OldManBrain))]
[RequireComponent(typeof(OldManActionDriver))]
public class OldManController : StateMachineController
{
    //--------------------------------- Data ------------------------------------------
    [SerializeField] private OldManConfigSO _config;
    //--------------------------------- Component ------------------------------------------
    private Health _health;
    private Mover _mover;
    private Attacker _attacker;
    private OldManBrain _brain;

    //--------------------------------- Public Parameter ------------------------------------------
    public Health Health => _health;
    public Mover Mover => _mover;

    protected override void Awake()
    {
        base.Awake();

        // 缓存组件
        _health = GetComponent<Health>();
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
            
            //Health作为通用组件来存在 不纳入状态机架构 独立初始化
            if (config is IHealthConfig healthConfig)
            {
                _health?.Initialize(healthConfig.MaxHealthAmount, healthConfig.MaxHealthAmount);
            }
        }
    }
}
