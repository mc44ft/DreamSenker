using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using DreamSenker.Combat.Health;
using DreamSenker.Managers;
using DreamSenker.Shared;

namespace DreamSenker.Characters.Bosses
{
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Health))]
[DisallowMultipleComponent]
public class Minion : MonoBehaviour, IDamageable, ITouchDamageable
{
    //--------------------------------- Child Component -----------------------------------
    [Space(5)]
    [Header("POINT")]
    [SerializeField] private Transform _rayCheckGround;
    [SerializeField] private Transform _rayCheckWall;
    [SerializeField] private Transform _slimeAmmoFireTransform;
    [SerializeField] private GameObject _selectedEffect;
    [Header("TRIGGER")]
    [SerializeField] private Minion_TriggerBig _triggerBig;
    [SerializeField] private Minion_TriggerSmall _triggerSmall;

    //--------------------------------- Assets -----------------------------------
    [Space(5)]
    [Header("SLIME AMMO")]
    [SerializeField] private GameObject _slimeAmmoPrefab;

    //--------------------------------- Component -----------------------------------
    [Header("COMPONENT")]
    private Rigidbody2D _rigidbody;
    private Health _health;
    private SpriteRenderer _spriteRenderer;

    //--------------------------------- Private Parameter -----------------------------------
    [Space(5)]
    [Header("BASIC DETAILS")]
    [SerializeField] private int _maxHealthAmount = 3;
    [SerializeField] private int _damage;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _knockbackForceValue = 10f;
    [SerializeField] private float _deathDuration = 1f;
    [Tooltip("发射间隔时间")]
    [SerializeField] private float _launchIntervalTime = 2f;

    private Material _material;

    //private Coroutine _launchSlimeCoroutine;
    private Coroutine _chasePlayerCoroutine;

    private float _launchIntervalTimer;
    /// <summary>
    /// 是否找到了玩家
    /// 在与玩家交互状态下 
    /// 到达平台边缘不转向而是停止移动
    /// 与玩家距离过近时也停止移动
    /// </summary>
    private bool _isFindedPlayer;
    private bool _cannotMove;//找到玩家
    private float _faceRight;
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _material = _spriteRenderer.material;
    }
    private void Start()
    {
        _health.Initialize(_maxHealthAmount, _maxHealthAmount);
        //默认关闭选中特效
        Deselect();

        _faceRight = transform.localScale.x;
        _launchIntervalTimer = 0;
    }
    private void Update()
    {
        Debug.DrawRay(_rayCheckGround.position, -Vector2.up * 1, Color.red);
        Debug.DrawRay(_rayCheckWall.position, Vector2.right * _faceRight, Color.red);

        RaycastHit2D hitInfo_ground = Physics2D.Raycast(_rayCheckGround.position, 
            -Vector2.up, 1, 1 << LayerMask.NameToLayer("GroundReal"));
        RaycastHit2D hitInfo_wall = Physics2D.Raycast(_rayCheckWall.position, 
            Vector2.right * _faceRight, 1.5f, 1 << LayerMask.NameToLayer("GroundReal"));

        if (hitInfo_ground.collider == null || hitInfo_wall.collider != null)//踩空了
        {
            if (!_isFindedPlayer)
            {
                //转向
                UpdateFace(-_faceRight);
            }
            else
            {
                //在和玩家交互的过程中 遇到平台边缘或者墙 停止移动
                _cannotMove = true;
            }
        }

        _launchIntervalTimer -= Time.deltaTime;
        if(GameManager.Instance.Player != null)
            LaunchSlimeAmmo(GameManager.Instance.Player.transform);
    }
    private void FixedUpdate()
    {
        if (!_cannotMove && !_health.IsDead)
        {
            _rigidbody.velocity = new Vector3(_moveSpeed * _faceRight, _rigidbody.velocity.y);
        }
    }
    public void StartChasePlayer(Transform player)
    {
        //找到玩家
        _isFindedPlayer = true;

        //开始追击玩家
        if (_chasePlayerCoroutine != null)
        {
            StopCoroutine(_chasePlayerCoroutine);
        }
        _chasePlayerCoroutine = StartCoroutine(ChasePlayerCoroutine(player));
    }
    public void StopChasePlayer()
    {
        //恢复正常
        _isFindedPlayer = false;
        _cannotMove = false;

        //停止追击玩家
        if (_chasePlayerCoroutine != null)
        {
            StopCoroutine(_chasePlayerCoroutine);
            _chasePlayerCoroutine = null;
        }
    }
    public void StartLaunchSlimeAmmo(Transform player)
    {
        //停止移动
        _cannotMove = true;

        //if (_launchSlimeCoroutine != null)
        //{
        //    StopCoroutine(_launchSlimeCoroutine);
        //    _launchSlimeCoroutine = null;
        //}
        //_launchSlimeCoroutine = StartCoroutine(LaunchSlimeAmmoCoroutine(player, _launchIntervalTime));
    }
    public void StopLaunchSlimeAmmo()
    {
        //恢复移动
        _cannotMove = false;

        //if (_launchSlimeCoroutine != null)
        //{
        //    StopCoroutine(_launchSlimeCoroutine);
        //    _launchSlimeCoroutine = null;
        //}
    }
    //private IEnumerator LaunchSlimeAmmoCoroutine(Transform player, float intervalTime = 2f)
    //{
    //    while (true && !_health.IsDead)
    //    {
    //        //确定玩家位置
    //        Vector3 targetPosition = player.position;
    //        //实例化史莱姆
    //        SlimeAmmo slimeAmmo = Instantiate(_slimeAmmoPrefab, _slimeAmmoFireTransform.position, Quaternion.identity).GetComponent<SlimeAmmo>();
    //        slimeAmmo.Init(_damage, new Vector2(_faceRight, 0));
    //        //向玩家位置抛出史莱姆
    //        slimeAmmo.Launch(targetPosition);

    //        yield return new WaitForSeconds(intervalTime);
    //    }
    //}
    public void LaunchSlimeAmmo(Transform player)
    {
        if (player == null) return;
        if(_launchIntervalTimer <= 0f && _cannotMove)
        {
            //确定玩家位置
            Vector3 targetPosition = player.position;
            //实例化史莱姆
            SlimeAmmo slimeAmmo = Instantiate(_slimeAmmoPrefab, _slimeAmmoFireTransform.position, Quaternion.identity).GetComponent<SlimeAmmo>();
            slimeAmmo.Init(_damage, new Vector2(_faceRight, 0));
            //向玩家位置抛出史莱姆
            slimeAmmo.Launch(targetPosition);

            _launchIntervalTimer = _launchIntervalTime;
        }
    }
    /// <summary>
    /// 在与玩家整个交互过程中 不断调整面朝向
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    private IEnumerator ChasePlayerCoroutine(Transform player)
    {
        while(true)
        {
            //决定怪物朝向
            float value = Mathf.Abs(transform.localScale.x);
            float newFace = (player.position.x - transform.position.x) > 0 ? value : -value;

            if(_faceRight != newFace)
            {
                UpdateFace(newFace);
            }
            yield return null;
        }
    }
    
    private void UpdateFace(float newFace)
    {
        _faceRight = newFace;
        transform.localScale = new Vector3(_faceRight, transform.localScale.y, transform.localScale.z);
    }
    public int GetTouchDamage()
    {
        return _damage;
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (_health.IsDead) return;

        _health.ApplyDamage(damage);

        if (_health.IsDead)
        {
            _material.DOFloat(1f, Settings.DissolveAmountString, _deathDuration).
                SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
        }

        StartCoroutine(TakeDamageRoutine(attackDirection));
    }
    private IEnumerator TakeDamageRoutine(Vector2 attackDirection)
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        _spriteRenderer.color = Color.white;
        //被击飞
        HelperUtilities.DoKnockback(_rigidbody, attackDirection, _knockbackForceValue);
    }
    public Vector2 GetPosition()
    {
        //这里不能检查gameObject != null
        //因为gameObject是该类的一个属性 当该类被销毁时 直接访问其属性会报错
        //所以这里直接检查this
        if(this)
        {
            return transform.position;
        }
        return Vector2.zero;
    }
    public void Select()
    {
        //选中该对象
        _selectedEffect.SetActive(true);
    }

    public void Deselect()
    {
        _selectedEffect.SetActive(false);
    }

    
}
}
