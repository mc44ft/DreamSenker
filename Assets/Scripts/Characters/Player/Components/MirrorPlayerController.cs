using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.Characters.Player
{
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class MirrorPlayerController : MonoBehaviour
{
    private Animator m_animator;
    private Animator m_realAnimator;
    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_realAnimator = transform.root.GetComponent<Animator>();
    }
    private void LateUpdate()
    {
        //同步本体的动画状态
        AnimatorStateInfo info = m_realAnimator.GetCurrentAnimatorStateInfo(0);
        m_animator.Play(info.fullPathHash, 0, info.normalizedTime);
        
    }
}
}
