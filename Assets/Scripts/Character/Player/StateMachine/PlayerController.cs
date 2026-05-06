using System;
using PlayArk.StateMachine;
using PlayArk.StateMachine.Utilities;
using UnityEngine;
#region Attribute
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent((typeof(Mover)))]
[RequireComponent((typeof(AnimationPlayer)))]
[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(PlayerHealth))]
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
    public PlayerHealth Health => _health;
    public Mover Mover => _mover;
    //--------------------------------- Private Parameter ------------------------------------------
    private PlayerHealth _health;
    private Mover _mover;
    private InputReader _inputReader;

    private FormStrategy _currentForm;
    private FormUnarmed _formUnarmed;
    private FormSword _formSword;
    private PlayerSaveData _playerSaveData;

    protected override void Awake()
    {
        base.Awake();

        // _health = GetComponent<PlayerHealth>();
        // _mover = GetComponent<Mover>();
        // _inputReader = GetComponent<InputReader>();
        // _formUnarmed = GetComponent<FormUnarmed>();
        // _formSword = GetComponent<FormSword>();
    }
    /// <summary>
    /// 外部初始化
    /// </summary>
    /// <param name="onFinished">初始化完成后调用</param>
    public void Initialize(Action onFinished)
    {
        _health = GetComponent<PlayerHealth>();
        _mover = GetComponent<Mover>();
        _inputReader = GetComponent<InputReader>();
        _formUnarmed = GetComponent<FormUnarmed>();
        _formSword = GetComponent<FormSword>();
        
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
