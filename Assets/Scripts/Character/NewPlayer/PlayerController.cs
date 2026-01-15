using System;
using PlayArk.StateMachine;
using UnityEngine;
#region Attribute
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent((typeof(Mover)))]
[RequireComponent((typeof(AnimationPlayer)))]
[RequireComponent(typeof(InputReader))]
[RequireComponent(typeof(Attacker))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(NewFormReal))]
[RequireComponent(typeof(NewFormMirror))]
[DisallowMultipleComponent]
#endregion
public class PlayerController : StateMachineController
{
    //--------------------------------- Data ------------------------------------------
    [SerializeField] private PlayerConfigSO _playerConfig;
    private PlayerSaveData _playerSaveData;
    
    //--------------------------------- Component ------------------------------------------
    //--------------------------------- Public Parameter ------------------------------------------
    public int FaceRight => _faceRight;
    public PlayerSaveData PlayerSaveData => _playerSaveData;
    public NewHealth Health => _health;
    public Mover Mover => _mover;
    public SpriteRenderer SpriteRenderer => _spriteRenderer;
    //--------------------------------- Private Parameter ------------------------------------------
    //这是用于镜像地图的隐藏分身 只是个特殊的效果 也不属于镜像形态 后期可以通过外部添加而不是关闭 这里先放在这里吧
    [SerializeField] private GameObject _mirrorGameObject;
    [SerializeField] private SpriteRenderer _shadowSR;
    private NewHealth _health;
    private Mover _mover;
    private SpriteRenderer _spriteRenderer;
    private NewFormStrategy _currentForm;
    private NewFormReal _formReal;
    private NewFormMirror _formMirror;
    private Material _shadowMaterial;
    
    private int _faceRight = 1;
    private void Awake()
    {
        _health = GetComponent<NewHealth>();
        _mover = GetComponent<Mover>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _formReal = GetComponent<NewFormReal>();
        _formMirror = GetComponent<NewFormMirror>();
        
        if(_playerConfig != null)
            _playerSaveData = _playerConfig.PlayerSaveData;
        
        _shadowMaterial = _shadowSR.material;
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
            if (_currentForm == _formReal) SwitchFormMirror();
            else SwitchFormReal();
        }
    }
    private void SwitchFormReal()
    {
        _currentForm = _formReal;
        _formReal.enabled = true;
        _formMirror.enabled = false;
        //依赖注入
        _currentForm.SwitchSetup(
            _playerConfig.PlayerConfig, 
            _playerConfig.RealFormConfig, 
            _playerConfig.PlayerSaveData);
    }

    private void SwitchFormMirror()
    {
        _currentForm = _formMirror;
        _formReal.enabled = false;
        _formMirror.enabled = true;
        //依赖注入
        _currentForm.SwitchSetup(
            _playerConfig.PlayerConfig, 
            _playerConfig.MirrorFormConfig,
            _playerConfig.PlayerSaveData);
    }
    public void SetMirrorActive(bool isActive)
    {
        _mirrorGameObject.SetActive(isActive);
    }
    public void SetShadowDarknessStrength(float strength)
    {
        _shadowMaterial.SetFloat("_DarknessStrength", strength);
    }
    protected override void Update()
    {
        base.Update();
        UpdateFace();
    }

    private void UpdateFace()
    {
        if (InputManager.Instance.HorizontalValue == 0) return;
        int newFaceRight = InputManager.Instance.HorizontalValue > 0 ? 1 : -1;

        //玩家输入方向改变时才执行
        if(newFaceRight != _faceRight)
        {
            _faceRight = newFaceRight;
            transform.localScale = new Vector3(_faceRight, 1, 1);
        }
    }
}
