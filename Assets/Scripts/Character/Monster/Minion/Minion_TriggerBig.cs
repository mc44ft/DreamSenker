using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_TriggerBig : MonoBehaviour
{
    private Minion m_minionBase;
    private CircleCollider2D m_collider;
    private void Awake()
    {
        m_minionBase = GetComponentInParent<Minion>();
        m_collider = GetComponent<CircleCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_minionBase.StartChasePlayer(collision.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            m_minionBase.StopChasePlayer();
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(m_collider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_collider.radius);
        }
        
    }
#endif
}
