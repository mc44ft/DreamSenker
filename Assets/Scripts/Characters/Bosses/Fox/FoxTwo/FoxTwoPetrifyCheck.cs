using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
[RequireComponent(typeof(Collider2D))]
public class FoxTwoPetrifyCheck : MonoBehaviour
{
    private Collider2D _collider;
    public Transform Target {  get; private set; }
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }
    /// <summary>
    /// 组件激活 可以触发Enter
    /// 组件失活 不能触发Exit
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            Target = collision.transform;
        }
    }
    private void Update()
    {
        if(_collider.isActiveAndEnabled == false && Target != null)
        {
            Target = null;
        }
    }
}
}
