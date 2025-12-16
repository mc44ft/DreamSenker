using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]
[DisallowMultipleComponent]
public class AttackCheckMirror : MonoBehaviour
{
    //[HideInInspector] 
    public List<Transform> MonsterList = new List<Transform>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            MonsterList.Add(collision.transform);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Monster"))
        {
            MonsterList.Remove(collision.transform);
        }
    }
}
