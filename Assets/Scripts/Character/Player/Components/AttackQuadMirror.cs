using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackQuadMirror : MonoBehaviour
{
    [SerializeField] private float m_breatheDuration = 2.0f;
    [Range(0f, 1f)]
    [SerializeField] private float m_minAlpha = 0.1f;
    //[Range(0f, 1f)]
    //[SerializeField] private float m_maxAlpha = 1.0f;
    private Material m_shieldMaterial;
    private void Awake()
    {
        m_shieldMaterial = GetComponent<Renderer>().material;
    }
    void Start()
    {
        //初始化
        m_shieldMaterial.SetFloat("_MasterAlpha", 1f);

        m_shieldMaterial.DOFloat(m_minAlpha, "_MasterAlpha", m_breatheDuration).
            SetEase(Ease.InOutSine).
            SetLoops(-1, LoopType.Yoyo);//-1表示无限循环 Yoyo是向溜溜球一样的循环方式
    }
    private void OnDestroy()
    {
        //在物体销毁时杀掉动画 保证安全
        m_shieldMaterial.DOKill();
    }
}
