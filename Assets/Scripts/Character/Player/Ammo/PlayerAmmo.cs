
using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class PlayerAmmo : MonoBehaviour
{
    private float m_ammoSpeed;
    private int m_ammoDamage;
    private Vector2 m_targetPosition;
    private Vector2 m_fireDirection;

    private Rigidbody2D m_rigidbody;
    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }
    public void Init(float ammoSpeed, int ammoDamage, Vector2 targetPosition)
    {
        m_ammoSpeed = ammoSpeed;
        m_ammoDamage = ammoDamage;
        m_targetPosition = targetPosition;
        m_fireDirection = (m_targetPosition - (Vector2)transform.position).normalized;
    }
    private void FixedUpdate()
    {
        Vector2 unitVector = m_rigidbody.position + m_ammoSpeed * Time.fixedDeltaTime * m_fireDirection;
        m_rigidbody.MovePosition(unitVector);

        //这里使用技能的平方来检测 不开根号更省性能
        if ((m_targetPosition - m_rigidbody.position).sqrMagnitude < 0.01f)
        {
            Destroy(gameObject);
        }
    }
    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Monster"))
    //     {
    //         if(collision.gameObject.TryGetComponent<IDamageable>(out IDamageable damageable))
    //         {
    //             //Buff脏代码
    //             if(GameManager.Instance.Player.IsDebuff && Random.value < 0.4f)
    //             {
    //                 collision.gameObject.AddComponent<MournfulGazeDebuff>();
    //             }
    //
    //             damageable.TakeDamage(m_ammoDamage, 
    //                 (transform.position.x - collision.transform.position.x) > 0 ? -Vector2.right : Vector2.right);
    //             Destroy(gameObject);
    //         }
    //     }
    // }
}
