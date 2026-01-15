
using PlayArk.StateMachine.Utilities;
using UnityEngine;
/// <summary>
/// 综合了协调者模式（导演-演员模式）和策略模式 还有依赖注入的技巧
/// 通过具体的形态（导演）来指挥功能组件（演员）来实现玩家双形态
/// 在InjectionConfig方法中通过依赖注入来将数据包注入到每个功能组件中
/// </summary>
[RequireComponent(typeof(Mover))]
[RequireComponent(typeof(AnimationPlayer))]
[RequireComponent(typeof(NewHealth))]
public abstract class NewFormStrategy : MonoBehaviour, IAction
{
    //------------------ 持有玩家数据包 -----------------
    protected PlayerConfig _playerConfig;
    protected PlayerFormConfig _formConfig;
    protected PlayerSaveData _playerSaveData;
    //------------------ 持有玩家身上的功能组件 -----------------
    protected Mover _mover;
    protected AnimationPlayer _animationPlayer;
    protected NewHealth _health;

    protected virtual void Awake()
    {
        _mover = GetComponent<Mover>();
        _animationPlayer = GetComponent<AnimationPlayer>();
        _health = GetComponent<NewHealth>();
    }

    public void SwitchSetup(PlayerConfig playerConfig, PlayerFormConfig formConfig, PlayerSaveData playerSaveData)
    {
        if(_playerConfig == null)
            _playerConfig = playerConfig;
        if(_formConfig == null)
            _formConfig = formConfig;
        if(_playerSaveData == null)
            _playerSaveData = playerSaveData;
        
        
        //设置初始的动画状态机
        _animationPlayer.SetRuntimeAnimatorController(formConfig.RuntimeAnimatorController);
        //重置跳跃次数
        _mover.ResetJumpCounter();
        //初始化健康值组件
        _health.Initialize(_formConfig.MaxHealthAmount, _playerSaveData.CurrentHealth);
        
        //依赖注入
        InjectionConfig(formConfig);
    }
    /// <summary>
    /// 把受击效果嵌入到动画内部
    /// </summary>
    private void GetHitAnimEvent()
    {
        if (!this.enabled) return;
        GameManager.Instance.CameraShake();
        GameManager.Instance.DoHitStop(_formConfig.GetHitStopTime);
    }
    /// <summary>
    /// 将数据包注入到功能组件中
    /// </summary>
    private void InjectionConfig(PlayerFormConfig config)
    {
        foreach (var component in GetComponents<IComponent>())
        {
            component.InjectionConfigBase(config);
        }
    }

    public abstract void DoAction(EAction action, string[] parameters);
}
