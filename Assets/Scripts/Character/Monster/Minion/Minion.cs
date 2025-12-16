using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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
    [SerializeField] private Transform m_rayCheckGround;
    [SerializeField] private Transform m_rayCheckWall;
    [SerializeField] private Transform m_slimeAmmoFireTransform;
    [SerializeField] private GameObject m_selectedEffect;

    //--------------------------------- Assets -----------------------------------
    [Space(5)]
    [Header("SLIME AMMO")]
    [SerializeField] private GameObject m_slimeAmmoPrefab;

    //--------------------------------- Component -----------------------------------
    [Header("COMPONENT")]
    private Rigidbody2D m_rigidbody;
    private Health m_health;
    private SpriteRenderer m_spriteRenderer;

    //--------------------------------- Private Parameter -----------------------------------
    [Space(5)]
    [Header("BASIC DETAILS")]
    [SerializeField] private int m_maxHealthAmount = 3;
    [SerializeField] private int m_damage;
    [SerializeField] private float m_moveSpeed;
    [SerializeField] private float m_knockbackForceValue = 10f;
    [SerializeField] private float m_deathDuration = 1f;
    [Tooltip("发射间隔时间")]
    [SerializeField] private float m_launchIntervalTime = 2f;

    private Material m_material;

    private Coroutine m_launchSlimeCoroutine;
    private Coroutine m_chasePlayerCoroutine;
    /// <summary>
    /// 是否找到了玩家
    /// 在与玩家交互状态下 
    /// 到达平台边缘不转向而是停止移动
    /// 与玩家距离过近时也停止移动
    /// </summary>
    private bool m_isFindedPlayer;
    private bool m_cannotMove;
    //private bool m_deathTriggered;
    private int m_faceRight = 1;
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
        m_health = GetComponent<Health>();
        m_spriteRenderer = GetComponent<SpriteRenderer>();

        m_material = m_spriteRenderer.material;
    }
    private void Start()
    {
        m_health.Initialize(m_maxHealthAmount, m_maxHealthAmount);
        //默认关闭选中特效
        Deselect();
    }
    private void Update()
    {
        Debug.DrawRay(m_rayCheckGround.position, -Vector2.up * 1, Color.red);
        Debug.DrawRay(m_rayCheckWall.position, Vector2.right * m_faceRight, Color.red);

        RaycastHit2D hitInfo_ground = Physics2D.Raycast(m_rayCheckGround.position, 
            -Vector2.up, 1, 1 << LayerMask.NameToLayer("GroundReal"));
        RaycastHit2D hitInfo_wall = Physics2D.Raycast(m_rayCheckWall.position, 
            Vector2.right * m_faceRight, 1.5f, 1 << LayerMask.NameToLayer("GroundReal"));

        if (hitInfo_ground.collider == null || hitInfo_wall.collider != null)//踩空了
        {
            if (!m_isFindedPlayer)
            {
                //转向
                UpdateFace(-m_faceRight);
            }
            else
            {
                //在和玩家交互的过程中 遇到平台边缘或者墙 停止移动
                m_cannotMove = true;
            }
        }
    }
    private void FixedUpdate()
    {
        if (!m_cannotMove && !m_health.IsDead)
        {
            m_rigidbody.velocity = new Vector3(m_moveSpeed * m_faceRight, m_rigidbody.velocity.y);
        }
    }
    public void StartChasePlayer(Transform player)
    {
        //找到玩家
        m_isFindedPlayer = true;

        
        if (m_chasePlayerCoroutine != null)
        {
            StopCoroutine(m_chasePlayerCoroutine);
        }
        m_chasePlayerCoroutine = StartCoroutine(ChasePlayerCoroutine(player));
    }
    public void StopChasePlayer()
    {
        //恢复正常
        m_isFindedPlayer = false;
        m_cannotMove = false;

        if (m_chasePlayerCoroutine != null)
        {
            StopCoroutine(m_chasePlayerCoroutine);
        }
    }
    public void StartLaunchSlimeAmmo(Transform player)
    {
        //停止移动
        m_cannotMove = true;

        if (m_launchSlimeCoroutine != null)
        {
            StopCoroutine(m_launchSlimeCoroutine);
        }
        m_launchSlimeCoroutine = StartCoroutine(LaunchSlimeAmmoCoroutine(player, m_launchIntervalTime));
    }
    public void StopLaunchSlimeAmmo()
    {
        //恢复移动
        m_cannotMove = false;

        if (m_launchSlimeCoroutine != null)
        {
            StopCoroutine(m_launchSlimeCoroutine);
        }
    }
    private IEnumerator LaunchSlimeAmmoCoroutine(Transform player, float intervalTime = 2f)
    {
        while (true && !m_health.IsDead)
        {
            //确定玩家位置
            Vector3 targetPosition = player.position;
            //实例化史莱姆
            SlimeAmmo slimeAmmo = Instantiate(m_slimeAmmoPrefab, m_slimeAmmoFireTransform.position, Quaternion.identity).GetComponent<SlimeAmmo>();
            slimeAmmo.Init(m_damage, new Vector2(m_faceRight, 0));
            //向玩家位置抛出史莱姆
            slimeAmmo.Launch(targetPosition);

            yield return new WaitForSeconds(intervalTime);
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
            int newFace = (player.position.x - transform.position.x) > 0 ? 1 : -1;

            if(m_faceRight != newFace)
            {
                UpdateFace(newFace);
            }
            yield return null;
        }
    }
    
    private void UpdateFace(int newFace)
    {
        m_faceRight = newFace;
        transform.localScale = new Vector3(m_faceRight, 1, 1);
    }
    public int GetTouchDamage()
    {
        return m_damage;
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (m_health.IsDead) return;

        m_health.ApplyDamage(damage);

        if (m_health.IsDead)
        {
            m_material.DOFloat(1f, Settings.DissolveAmountString, m_deathDuration).
                SetEase(Ease.InOutQuad).OnComplete(() =>
                {
                    Destroy(gameObject);
                });
        }

        StartCoroutine(TakeDamageRoutine(attackDirection));
    }
    private IEnumerator TakeDamageRoutine(Vector2 attackDirection)
    {
        m_spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        m_spriteRenderer.color = Color.white;
        //被击飞
        HelperUtilities.DoKnockback(m_rigidbody, attackDirection, m_knockbackForceValue);
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
        m_selectedEffect.SetActive(true);
    }

    public void Deselect()
    {
        m_selectedEffect.SetActive(false);
    }

    
}
