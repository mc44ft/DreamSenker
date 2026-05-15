using System;
using PlayArk.StateMachine;
using PlayArk.StateMachine.Utilities;
using UnityEngine;

using DreamSeeker.Combat.Health;
using DreamSeeker.Data;
using DreamSeeker.Data.Configs.Character.Player;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Player
{
#region Attribute
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent((typeof(Mover)))]
[RequireComponent(typeof(Jumper))]
[RequireComponent((typeof(AnimationPlayer)))]
[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DamageableHealth))]
[RequireComponent(typeof(FormUnarmed))]
[RequireComponent(typeof(FormSword))]
[DisallowMultipleComponent]
#endregion
public class PlayerController : StateMachineController, IAction
{
    //--------------------------------- Data ------------------------------------------
    [SerializeField] private PlayerConfigSO _playerConfig;
    //--------------------------------- Component ------------------------------------------
    //--------------------------------- Public Parameter ------------------------------------------
    public PlayerSaveData PlayerSaveData => _playerSaveData;
    public DamageableHealth DamageableHealth => _damageableHealth;
    public Mover Mover => _mover;

    public Transform CameraFollowTran => _cameraFollow != null ? _cameraFollow.transform : null;
    //--------------------------------- Private Parameter ------------------------------------------
    private DamageableHealth _damageableHealth;
    private Mover _mover;
    private InputReader _inputReader;

    private FormStrategy _currentForm;
    private FormUnarmed _formUnarmed;
    private FormSword _formSword;
    private PlayerSaveData _playerSaveData;

    private GameObject _cameraFollow;

    protected override void Awake()
    {
        base.Awake();

        _damageableHealth = GetComponent<DamageableHealth>();
        _mover = GetComponent<Mover>();
        _inputReader = GetComponent<InputReader>();
        _formUnarmed = GetComponent<FormUnarmed>();
        _formSword = GetComponent<FormSword>();
        
        //这里创建一个用于缓慢跟随玩家翻转的物体，使虚拟摄像机跟随此物体，能够达到一个缓动跟随的效果
        //具体表现就是：玩家转向时，摄像机能自动跑到玩家面前一段距离
        _cameraFollow = new GameObject("CameraFollow");
        _cameraFollow.AddComponent<CameraFollowObject>().SetUp(this.transform);
    }
    private void OnDestroy()
    {
        Destroy(_cameraFollow);
    }

    private void OnEnable()
    {
        // 订阅Health事件，触发玩家血条UI刷新
        _damageableHealth.GetComponent<Health>().OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        // 订阅Health事件，触发玩家血条UI刷新
        _damageableHealth.GetComponent<Health>().OnHealthChanged -= OnHealthChanged;
    }
    private void OnHealthChanged(int maxHealth, int currentHealth)
    {
        EventCenter.Instance.EventTrigger(
            E_EventType.Player_HealthUpdate,
            this,
            new PlayerHealthUpdateEventArgs(maxHealth, currentHealth));
    }
    /// <summary>
    /// 外部初始化
    /// </summary>
    /// <param name="onFinished">初始化完成后调用</param>
    public void Initialize(Action onFinished)
    {
        LoadData();
        //切换到初始形态
        SwitchForm();
        
        
        
        onFinished?.Invoke();
    }
    private void LoadData()
    {
        //加载玩家数据
        RunningDataManager.Instance.LoadData(out PlayerSaveData playerSaveData);

        if (playerSaveData == null)//无存档数据
            _playerSaveData = HelpUtilities.DeepCopy(_playerConfig.PlayerSaveData);
        else//有存档数据
            _playerSaveData = playerSaveData;
    }
    public void SwitchForm()
    {
        //初次切换形态
        if (_currentForm == null)
        {
            SwitchFormReal();
        }
        else
        {
            if (_currentForm == _formUnarmed) SwitchFormMirror();
            else SwitchFormReal();
        }
    }
    private void SwitchFormReal()
    {
        _currentForm = _formUnarmed;
        _formUnarmed.enabled = true;
        _formSword.enabled = false;
        //依赖注入
        _currentForm.SwitchSetup(
            _playerConfig.PlayerConfig, 
            _playerConfig.FormConfigUnarmed, 
            _playerConfig.PlayerSaveData);
    }

    private void SwitchFormMirror()
    {
        _currentForm = _formSword;
        _formUnarmed.enabled = false;
        _formSword.enabled = true;
        //依赖注入
        _currentForm.SwitchSetup(
            _playerConfig.PlayerConfig, 
            _playerConfig.FormConfigSword,
            _playerConfig.PlayerSaveData);
    }

    private void SwitchFormHandle(string buttomName)
    {
        if (_inputReader.CheckKeyCodePressed(buttomName))
        {
            Debug.Log("Switch form handle");
            SwitchForm();
        }
    }
    public void DoAction(EAction action, string[] parameters)
    {
        switch (action)
        {
            case EAction.SwitchFormHandle:
                SwitchFormHandle(parameters[0]);
                break;
        }
    }
}
}
