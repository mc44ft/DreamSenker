using UnityEngine;

using DreamSenker.Combat.Health;
using DreamSenker.Shared;

namespace DreamSenker.Characters.Bosses
{
[RequireComponent(typeof(PolygonCollider2D))]
public class TouchHit : MonoBehaviour
{
    private float m_timer = 0f;
    private Health m_health;
    private void Awake()
    {
        m_health = GetComponentInParent<Health>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag) && !m_health.IsDead)
        {
            Vector2 attackDirection = collision.transform.position.x - transform.position.x > 0 ? Vector2.right : -Vector2.right;
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(transform.root.GetComponent<ITouchDamageable>().GetTouchDamage(), attackDirection);
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        //解决玩家卡在怪物内部 不触发伤害的问题
        //但是又不能频繁触发 间隔一定时间 将玩家击退即可
        if (collision.gameObject.CompareTag(Settings.PlayerTag) && !m_health.IsDead)
        {
            if(m_timer <= 0f)
            {
                Vector2 attackDirection = collision.transform.position.x - transform.position.x > 0 ? Vector2.right : -Vector2.right;
                
                if(collision.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(transform.root.GetComponent<ITouchDamageable>().GetTouchDamage(), attackDirection);
                }
                m_timer = 1f;
            }
            m_timer -= Time.deltaTime;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        //保证第一次触发及时
        if (collision.gameObject.CompareTag(Settings.PlayerTag) && !m_health.IsDead)
        {
            m_timer = 0f;
        }
    }

}
}
