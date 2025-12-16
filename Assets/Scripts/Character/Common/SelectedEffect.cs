using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class SelectedEffect : MonoBehaviour
{
    
    [SerializeField] private SpriteRenderer m_mainSpriteRenderer;
    private SpriteRenderer m_spriteRenderer;

    private void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Update()
    {
        //同步帧动画 保持轮廓同步
        m_spriteRenderer.sprite = m_mainSpriteRenderer.sprite;
    }
}
