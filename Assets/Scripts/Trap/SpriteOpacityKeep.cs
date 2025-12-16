using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOpacityKeep : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _targetSR;
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void LateUpdate()
    {
        _spriteRenderer.color = _targetSR.color;
    }
}
