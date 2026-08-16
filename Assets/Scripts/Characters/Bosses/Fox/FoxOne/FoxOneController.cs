using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using DreamSeeker.Combat.Health;
using DreamSeeker.Data.Configs.Character.Monster.Fox;
using DreamSeeker.Framework.Events;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
public class FoxOneController : FoxBoss
{
    //------------------------ Children Components --------------------------
    


    //------------------------ Private Parameter --------------------------
    private E_FoxOneMode _currentMode;
    private MachineManager<FoxOneController> _machineManager;



    //------------------------ Public Parameter --------------------------
    public FoxOneConfigSO FoxOneConfig {  get; private set; }

    //-----------------------Machine State --------------------------
    public FoxOneDeathState FoxOneDeathState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        _machineManager = new MachineManager<FoxOneController>();

        FoxOneDeathState = new FoxOneDeathState(this, _machineManager);

    }
    /// <summary>
    /// 以分身形态初始化一尾
    /// </summary>
    public void InitializeAsClone(Transform player, IFoxBossRoom bossRoom, Vector3 showPosition)
    {
        _currentMode = E_FoxOneMode.Clone;
        Initialize(player, bossRoom);
        
        //初始化血量
        Health.Initialize(FoxOneConfig.CloneFormMaxHealthAmount, FoxOneConfig.CloneFormMaxHealthAmount);
        //设置透明度
        this.SpriteRenderer.color = new Color(1, 1, 1, 0.8f);
        //初始隐匿
        Material.SetFloat(Settings.DissolveAmountString, 1f);
        //现身
        StartShow(showPosition);
        StartFloating();
    }
    /// <summary>
    /// 以Boss形态初始化一尾
    /// </summary>
    public void InitializeAsBoss(Transform player, IFoxBossRoom bossRoom)
    {
        _currentMode = E_FoxOneMode.Boss;
        Initialize(player, bossRoom);

        Health.Initialize(FoxOneConfig.MaxHealthAmount, FoxOneConfig.MaxHealthAmount);

    }
    public override void Initialize(Transform player, IFoxBossRoom foxBossRoom)
    {
        base.Initialize(player, foxBossRoom);
        FoxOneConfig = _foxConfig as FoxOneConfigSO;
    }
    private void Update()
    {

        _machineManager.LogicUpdate();
    }
    private void FixedUpdate()
    {
        _machineManager.PhysicsUpdate();
    }
    public override void TakeDamage(int damage, Vector2 attackDirection)
    {
        base.TakeDamage(damage, attackDirection);
        if (Health.IsDead)
        {
            if(_currentMode == E_FoxOneMode.Clone)
            {
                EventBus.Publish(new GameBossDiedEvent(EBossType.FoxClone, gameObject));
                _machineManager.TransitionTo(FoxOneDeathState);
            }
            
        }
    }
    public enum E_FoxOneMode
    {
        /// <summary>
        /// 作为二尾的分身形式出现
        /// </summary>
        Clone,
        /// <summary>
        /// 作为独立的分身形式出现
        /// </summary>
        Boss,
    }
}
}
