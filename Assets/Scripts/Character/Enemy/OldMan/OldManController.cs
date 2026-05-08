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

#if UNITY_EDITOR
    /// <summary>
    /// 绘制AI检测范围可视化
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_config == null)
            return;

        var config = _config.Config;

        // 检测范围 - 黄色
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, config.DetectRange);

        // 攻击范围 - 红色
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, config.AttackRange);

        // 丢失目标范围 - 紫色
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, config.LoseTargetRange);
    }
#endif
}
