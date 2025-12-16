using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Health))]
public class SpiderSilk : MonoBehaviour, IDamageable
{

    private Vector3 m_targetPosition;
    private float m_silkContinueTime;
    private float m_silkSpeed;
    private float m_silkClampSpeedRate;


    private bool m_isWebSkill;
    private PlayerController m_playerController;

    private Rigidbody2D m_rb;
    //private CircleCollider2D m_collider;
    private Health m_health;
    private void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        //m_collider = GetComponent<CircleCollider2D>();
        m_health = GetComponent<Health>();
    }
    public void Init(Vector3 targetPosition, float silkContinueTime, float silkSpeed, float silkClampSpeedRate, int silkHealthAmount)
    {
        m_targetPosition = targetPosition;
        m_silkContinueTime = silkContinueTime;
        m_silkSpeed = silkSpeed;
        m_silkClampSpeedRate = silkClampSpeedRate;

        m_health.Initialize(silkHealthAmount, silkHealthAmount);
    }
    private void FixedUpdate()
    {
        if (!m_isWebSkill)
        {
            Vector2 unitVector = (m_targetPosition - transform.position).normalized;
            m_rb.MovePosition(m_rb.position + unitVector * m_silkSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(m_rb.position, m_targetPosition) < 0.1f)
            {
                m_isWebSkill = true;
                Destroy(gameObject, m_silkContinueTime);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            //限制玩家速度
            m_playerController = collision.GetComponent<PlayerController>();
            m_playerController.MoveSpeedMultiplier = m_silkClampSpeedRate;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            if (m_playerController != null)
            {
                //恢复玩家速度
                m_playerController.MoveSpeedMultiplier = 1f;
            }
        }
    }
    private void OnDestroy()
    {
        if(m_playerController != null)
        {
            //恢复玩家速度
            m_playerController.MoveSpeedMultiplier = 1f;
        }
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        m_health.ApplyDamage(1);

        if (m_health.IsDead)
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void Select()
    {
        
    }

    public void Deselect()
    {
        
    }
}
