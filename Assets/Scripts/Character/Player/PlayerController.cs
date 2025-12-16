using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IDamageable
{
    //--------------------------------- Data ------------------------------------------
    [field: SerializeField] public PlayerConfigSO PlayerConfig;
    public PlayerSaveData PlayerSaveData { get; private set; }

    //--------------------------------- Child Component ------------------------------------------
    [field: SerializeField] public BoxCollider2D AttackCheckCollider_Real {  get; private set; }
    [field: SerializeField] public CircleCollider2D AttackCheckCollider_Mirror {  get; private set; }
    [field: SerializeField] public AttackCheckMirror AttackCheck_Mirror {  get; private set; }
    [field: SerializeField] public GameObject AttackQuad_Mirror { get; private set; }
    [SerializeField] private GameObject _selectedEffect;
    [SerializeField] private GameObject _mirrorGameObject;
    [SerializeField] private SpriteRenderer _shadowSR;
    //--------------------------------- Component ------------------------------------------

    public Animator Animator {  get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public BoxCollider2D BoxCollider { get; private set; }
    public SpriteRenderer SpriteRenderer { get; private set; }
    public Health Health { get; private set; }

    //--------------------------------- Public Parameter ------------------------------------------
    public bool IsGround { get; private set; }
    public bool IsGetHit {  get; set; }
    public int JumpCounter { get; set; }
    public int FaceRight { get; private set; } = 1; //玩家面朝向
    public float MoveSpeedMultiplier { get; set; } = 1f;//玩家移动的速度乘数
    public Vector2 GethitDirection { get; private set; }
    public FormStrategy CurrentFormStrategy { get; private set; }
    public bool IsDebuff { get; private set; } = false;

    //--------------------------------- Private Parameter ------------------------------------------
    private MachineManager<PlayerController> _machineManager;
    private bool _previourIsGround;

    private PlayerRealForm _realForm;
    private PlayerMirrorForm _mirrorForm;
    private Material _shadowMaterial;
    //--------------------------------- State Instance ------------------------------------------
    public PlayerAttackState PlayerAttackState { get; private set; }
    public PlayerDeathState PlayerDeathState { get; private set; }
    public PlayerFallState PlayerFallState { get; private set; }
    public PlayerGetHitState PlayerGetHitState { get; private set; }
    public PlayerIdleState PlayerIdleState { get; private set; }
    public PlayerJumpState PlayerJumpState { get; private set; }
    public PlayerRunState PlayerRunState { get; private set; }
    public PlayerTransitionState PlayerTransitionState { get; private set; }

    private void Awake()
    {
        //加载组件引用
        Animator = GetComponent<Animator>();
        Rigidbody = GetComponent<Rigidbody2D>();
        BoxCollider = GetComponent<BoxCollider2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Health = GetComponent<Health>();

        //实例化状态机管理器
        _machineManager = new MachineManager<PlayerController>();

        //实例化状态
        PlayerAttackState = new PlayerAttackState(this, _machineManager);
        PlayerDeathState = new PlayerDeathState(this, _machineManager);
        PlayerFallState = new PlayerFallState(this, _machineManager);
        PlayerGetHitState = new PlayerGetHitState(this, _machineManager);
        PlayerIdleState = new PlayerIdleState(this, _machineManager);
        PlayerJumpState = new PlayerJumpState(this, _machineManager);
        PlayerRunState = new PlayerRunState(this, _machineManager);
        PlayerTransitionState = new PlayerTransitionState(this, _machineManager);

        //实例化形态
        _realForm = new PlayerRealForm(this, _machineManager);
        _mirrorForm = new PlayerMirrorForm(this, _machineManager);

        _shadowMaterial = _shadowSR.material;
    }
    private void OnEnable()
    {
        //玩家关心加成事件
        EventCenter.Instance.AddEventListener<GameBonusEffectEventArgs>(E_EventType.Game_BonusEffect, OnGameBonusEffect);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<GameBonusEffectEventArgs>(E_EventType.Game_BonusEffect, OnGameBonusEffect);
    }
    private void OnGameBonusEffect(object eventSender, GameBonusEffectEventArgs args)
    {
        switch (args.BonusEffectType)
        {
            case E_BonusEffectType.None:
                break;
            case E_BonusEffectType.RestoreHealth:
                Health.RestoreHealth(args.RestoreAmount);
                EventCenter.Instance.EventTrigger(
                    E_EventType.Player_HealthUpdate, this, new PlayerHealthUpdateEventArgs(Health.MaxHealthAmount, Health.CurrentHealthAmount));
                break;
            case E_BonusEffectType.RestoreMana:
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 外部初始化
    /// </summary>
    /// <param name="onFinished">初始化完成后调用</param>
    public void Initialize(Action onFinished)
    {
        LoadData();

        //关闭Mirror镜像
        SetMirrorActive(false);
        //设置初始状态为一形态
        CurrentFormStrategy = _realForm;
        //设置初始的动画状态机
        Animator.runtimeAnimatorController = CurrentFormStrategy.PlayerConfig.RuntimeAnimatorController;
        //初始化跳跃次数
        JumpCounter = CurrentFormStrategy.PlayerConfig.JumpCount;
        //初始化健康值组件
        Health.Initialize(PlayerConfig.MaxHealthAmount, PlayerSaveData.CurrentHealth);
        //默认关闭Mirror选中特效
        Deselect();
        //初始化状态机 设置玩家的首个状态为Idle
        _machineManager.Initialize(PlayerIdleState);

        onFinished?.Invoke();
    }
    private void LoadData()
    {
        //加载玩家数据
        RunningDataManager.Instance.LoadData(out PlayerSaveData playerSaveData);

        if (playerSaveData == null)//无存档数据
        {
            PlayerSaveData = HelpUtilities.DeepCopy(PlayerConfig.PlayerSaveData);
        }
        else//有存档数据
        {
            PlayerSaveData = playerSaveData;
        }
    }
    private void Update()
    {
        CheckIsGround();
        ResetJumpCounter();
        //全域拦截
        CurrentFormStrategy.GlobalUpdate();

        _machineManager.LogicUpdate();
    }
    private void CheckIsGround()
    {
        _previourIsGround = IsGround;
        //检测接地
        IsGround = Physics2D.OverlapBox(transform.position, new Vector2(0.6f, 0.05f), 0, PlayerConfig.PlayerGroundLayerMask);
    }
    private void ResetJumpCounter()
    {
        if (_previourIsGround != IsGround)
            if (IsGround)
                JumpCounter = CurrentFormStrategy.PlayerConfig.JumpCount;
    }
    private void FixedUpdate()
    {
        UpdateVelocity();

        _machineManager.PhysicsUpdate();
    }
    private void UpdateVelocity()
    {
        //限制下落和跳跃的最大速度
        float fallVelocityY = Mathf.Clamp(Rigidbody.velocity.y, -PlayerConfig.MaxGravityScale, PlayerConfig.MaxGravityScale);
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, fallVelocityY);
    }
    public void UpdateFace()
    {
        if (InputManager.Instance.HorizontalValue == 0) return;
        int newFaceRight = InputManager.Instance.HorizontalValue > 0 ? 1 : -1;

        //玩家输入方向改变时才执行
        if(newFaceRight != FaceRight)
        {
            FaceRight = newFaceRight;
            transform.localScale = new Vector3(FaceRight, 1, 1);
        }
    }
    public void AddDebuff(bool isDebuff)
    {
        IsDebuff = isDebuff;
    }
    public void SwitchForm()
    {
        CurrentFormStrategy = CurrentFormStrategy == _realForm ? _mirrorForm : _realForm;

        //切换动画状态机
        Animator.runtimeAnimatorController = CurrentFormStrategy.PlayerConfig.RuntimeAnimatorController;
        //刷新跳跃次数
        JumpCounter = CurrentFormStrategy.PlayerConfig.JumpCount;
    }
    public void SetMirrorActive(bool isActive)
    {
        _mirrorGameObject.SetActive(isActive);
    }
    public void SetShadowDarknessStrength(float strength)
    {
        _shadowMaterial.SetFloat("_DarknessStrength", strength);
    }
    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        //防止同时多次伤害
        //处于过渡状态时也不会受到伤害
        if (IsGetHit || _machineManager.CurrentState == PlayerTransitionState)
            return;

        //更新本次冲击方向
        GethitDirection = attackDirection;

        Health.ApplyDamage(damage);

        //通知外部UI更新血条
        EventCenter.Instance.EventTrigger(
            E_EventType.Player_HealthUpdate, this, new PlayerHealthUpdateEventArgs(Health.MaxHealthAmount, Health.CurrentHealthAmount));
        IsGetHit = true;

        if (Health.IsDead)
        {
            //读档重来
            GameManager.Instance.LoadGame();
        }
    }
    public Vector2 GetPosition()
    {
        return transform.position;
    }
    public void Select()
    {
        _selectedEffect.SetActive(true);
    }
    public void Deselect()
    {
        _selectedEffect.SetActive(false);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(CurrentFormStrategy == _realForm)
        {
            //绘制玩家的攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(AttackCheckCollider_Real.transform.position, AttackCheckCollider_Real.bounds.size);
        }
        else if(CurrentFormStrategy == _mirrorForm)
        {
            //绘制玩家的攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(AttackCheckCollider_Mirror.transform.position, AttackCheckCollider_Mirror.radius);
        }
    }
#endif
}