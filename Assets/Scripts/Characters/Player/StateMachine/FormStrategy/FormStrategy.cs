
using PlayArk.StateMachine.Utilities;
using UnityEngine;
using DreamSeeker.Data.Configs.Character.Player;
using DreamSeeker.Data.Runtime;

/// <summary>
/// 综合了协调者模式（导演-演员模式）和策略模式 还有依赖注入的技巧
/// 通过具体的形态（导演）来指挥功能组件（演员）来实现玩家双形态
/// 在InjectionConfig方法中通过依赖注入来将数据包注入到每个功能组件中
///
/// 玩家形态策略类
/// </summary>

namespace DreamSeeker.Characters.Player
{
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(Jumper))]
[RequireComponent(typeof(AnimationPlayer))]
[RequireComponent(typeof(DamageableHealth))]
public abstract class FormStrategy : MonoBehaviour, IAction
{
    //------------------ 持有玩家数据包 -----------------
    protected PlayerConfig _playerConfig;
    protected PlayerFormConfig _formConfig;
    protected PlayerSaveData _playerSaveData;
    //------------------ 持有玩家身上的基础功能组件 -----------------
    protected Mover _mover;
    protected Jumper _jumper;
    protected AnimationPlayer _animationPlayer;
    protected DamageableHealth _health;

    protected virtual void Awake()
    {
        _mover = GetComponent<Mover>();
        _jumper = GetComponent<Jumper>();
        _animationPlayer = GetComponent<AnimationPlayer>();
        _health = GetComponent<DamageableHealth>();
    }

    public void SwitchSetup(PlayerConfig playerConfig, PlayerFormConfig formConfig, PlayerSaveData playerSaveData)
    {
        _playerConfig ??= playerConfig;
        _formConfig ??= formConfig;
        _playerSaveData ??= playerSaveData;

        //依赖注入
        InjectionConfig(formConfig);

        //设置初始的动画状态机
        _animationPlayer.SetRuntimeAnimatorController(formConfig.RuntimeAnimatorController);
        //重置跳跃次数
        _jumper.ResetJumpCounter();
        // 通过 DamageableHealth 初始化底层 Health 组件
        _health.Initialize(_formConfig.MaxHealthAmount, _playerSaveData.CurrentHealth);
    }
    
    /// <summary>
    /// 将数据包注入到功能组件中
    /// </summary>
    private void InjectionConfig(PlayerFormConfig config)
    {
        //准备适配器
        PlayerAttackConfigAdapter attackAdapter = new(config, _playerSaveData);
        
        
        foreach (var component in GetComponents<IComponent>())
        {
            if (component is Attacker attacker)//为Attacker组件注入制作完成的适配器
                attacker.InjectionConfig(attackAdapter);
            else
                component.InjectionConfigBase(config);
            
        }
    }

    public abstract void DoAction(EAction action, string[] parameters);
}
}
