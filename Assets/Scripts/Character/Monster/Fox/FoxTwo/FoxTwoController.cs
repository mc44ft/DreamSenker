using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections;
using UnityEngine;

public class FoxTwoController : FoxBoss
{
    
    //-------------------- Children  Components ----------------------
    [field: SerializeField] public Transform BoomFirePoint {  get; private set; }
    [field: SerializeField] public BoxCollider2D ScratchCheckCollider2D {  get; private set; }
    [field: SerializeField] public FoxTwoPetrifyCheck PetrifyCheck {  get; private set; }

    //-------------------- Public Parameter ----------------------
    public FoxTwoConfigSO FoxTwoConfig {  get; private set; }

    //-------------------- Private Parameter ----------------------
    private MachineManager<FoxTwoController> _machineManager;
    //技能冷却计时
    private float _skillJumpCooldownTimer;
    private float _skillDashCooldownTimer;
    private float _skillCloneCooldownTimer;
    private float _skillBoomCooldownTimer;
    private float _skillPetrifyCooldownTimer;

    //-------------------- State Instance ----------------------
    public FoxTwoHideState HideState {  get; private set; }
    public FoxTwoSkillJumpState SkillJumpState { get; private set; }
    public FoxTwoSkillDashState SkillDashState { get; private set; }
    public FoxTwoSkillBoomState SkillBoomState { get; private set; }
    public FoxTwoSkillCloneState SkillCloneState { get; private set; }
    public FoxOneSkillPetrifyState SkillPetrifyState { get; private set; }
    public FoxTwoDeathState DeathState { get; private set; }

    
    protected override void Awake()
    {
        base.Awake();

        FoxTwoConfig = _foxConfig as FoxTwoConfigSO;

        _machineManager = new MachineManager<FoxTwoController>();

        HideState = new FoxTwoHideState(this, _machineManager);
        SkillJumpState = new FoxTwoSkillJumpState(this, _machineManager);
        SkillDashState = new FoxTwoSkillDashState(this, _machineManager);
        SkillBoomState = new FoxTwoSkillBoomState(this, _machineManager);
        SkillCloneState = new FoxTwoSkillCloneState(this, _machineManager);
        SkillPetrifyState = new FoxOneSkillPetrifyState(this, _machineManager);
        DeathState = new FoxTwoDeathState(this, _machineManager);
    }
    public override void Initialize(Transform player, IFoxBossRoom foxBossRoom)
    {
        base.Initialize(player, foxBossRoom);

        _machineManager.TransitionTo(HideState);

        Health.Initialize(FoxTwoConfig.MaxHealthAmount, FoxTwoConfig.MaxHealthAmount);

        //测试代码
        ResetSkillCooldownTimerAll();
    }
    private void Update()
    {
        UpdateSkillCooldownTimer();

        _machineManager.LogicUpdate();
    }
    private void FixedUpdate()
    {
        _machineManager.PhysicsUpdate();
    }
    
    
    /// <summary>
    /// 重置所有技能冷却
    /// </summary>
    public void ResetSkillCooldownTimerAll()
    {
        ResetSkillCooldownTimer(SkillCloneState);
        ResetSkillCooldownTimer(SkillBoomState);
        ResetSkillCooldownTimer(SkillJumpState);
        ResetSkillCooldownTimer(SkillDashState);
        ResetSkillCooldownTimer(SkillPetrifyState);
    }
    public void ResetSkillCooldownTimer(StateBase<FoxTwoController> skillState)
    {
        if (skillState == SkillCloneState)
            _skillCloneCooldownTimer = FoxTwoConfig.SkillCloneCooldown;
        else if (skillState == SkillBoomState)
            _skillBoomCooldownTimer = FoxTwoConfig.SkillBoomCooldown;
        else if (skillState == SkillJumpState)
            _skillJumpCooldownTimer = FoxTwoConfig.SkillJumpCooldown;
        else if (skillState == SkillDashState)
            _skillDashCooldownTimer = FoxTwoConfig.SkillDashCooldown;
        else if (skillState == SkillPetrifyState)
            _skillPetrifyCooldownTimer = FoxTwoConfig.SkillPetrifyCooldown;
    }
    /// <summary>
    /// 得到冷却结束的技能 有优先级
    /// </summary>
    /// <returns></returns>
    public bool TryGetCooldownFinishedSkill(out StateBase<FoxTwoController> skillState)
    {
        skillState = null;
        if (_skillCloneCooldownTimer <= 0f &&
            Health.CurrentHealthAmount <=
            FoxTwoConfig.MaxHealthAmount * FoxTwoConfig.SkillCloneTriggerHealthPercent) //最优先分身技能
        {
            skillState = SkillCloneState;
            return true;
        }
        if (_skillBoomCooldownTimer <= 0f)//次优先火球技能
        {
            skillState = SkillBoomState;
            return true;
        }
        if (_skillPetrifyCooldownTimer <= 0f)//次次优先石化技能
        {
            skillState = SkillPetrifyState;
            return true;
        }
        if (_skillJumpCooldownTimer <= 0f)
        {
            skillState = SkillJumpState;
            return true;
        }
        if (_skillDashCooldownTimer <= 0f)
        {
            skillState = SkillDashState;
            return true;
        }
        return false;
    }
    /// <summary>
    /// 检查指定技能冷却是否结束
    /// </summary>
    /// <param name="skillState"></param>
    /// <returns></returns>
    public bool CheckSkillCooldownFinished(StateBase<FoxTwoController> skillState)
    {
        if (skillState == SkillCloneState)
            return _skillCloneCooldownTimer <= 0f;
        else if (skillState == SkillBoomState)
            return _skillBoomCooldownTimer <= 0f;
        else if (skillState == SkillPetrifyState)
            return _skillPetrifyCooldownTimer <= 0f;
        else if (skillState == SkillJumpState)
            return _skillJumpCooldownTimer <= 0f;
        else if (skillState == SkillDashState)
            return _skillDashCooldownTimer <= 0f;
        return false;
    }
    
    /// <summary>
    /// 技能冷却计时
    /// </summary>
    private void UpdateSkillCooldownTimer()
    {
        _skillJumpCooldownTimer -= Time.deltaTime;
        _skillDashCooldownTimer -= Time.deltaTime;
        _skillCloneCooldownTimer -= Time.deltaTime;
        _skillBoomCooldownTimer -= Time.deltaTime;
        _skillPetrifyCooldownTimer -= Time.deltaTime;
    }
    
    public override void TakeDamage(int damage, Vector2 attackDirection)
    {
        base.TakeDamage(damage, attackDirection);
        if (Health.IsDead)
        {
            //退出状态并不会退出协程
            //可以在每个拥有协程的状态的Exit方法内停止协程
            //也可以直接在死亡时清除所有协程
            StopAllCoroutines();

            _machineManager.TransitionTo(DeathState);
        }
    }
}
