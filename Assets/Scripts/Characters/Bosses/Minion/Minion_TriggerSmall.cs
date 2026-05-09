using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.Characters.Bosses
{
public class Minion_TriggerSmall : MonoBehaviour
{
    private Minion _minionBase;
    private CircleCollider2D _collider;

    private void Awake()
    {
        _minionBase = GetComponentInParent<Minion>();
        _collider = GetComponent<CircleCollider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _minionBase.StartLaunchSlimeAmmo(collision.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _minionBase.StopLaunchSlimeAmmo();
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if(_minionBase != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _collider.radius);
        }
        
    }
#endif
}
}
