using DG.Tweening;
using System;
using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]
[DisallowMultipleComponent]
public class SlimeAmmo : MonoBehaviour
{
    [SerializeField] private GameObject m_shadowPrefab;

    [Tooltip("抛物线高度")]
    private float m_jumpPower = 5f;
    [Tooltip("飞行时间")]
    private float m_duration = 1.5f;
    [Tooltip("飞行过程中是否旋转")]
    private bool m_enableRotation = true;
    [Tooltip("旋转角度")]
    private Vector3 m_rotateAmout = new Vector3(0, 0, -360);

    //阴影位置
    private Transform m_shadowTran;

    //子弹数据
    private int m_damage;
    private Vector2 m_ammoDirection;


    public void Init(int damage, Vector2 ammoDirection)
    {
        m_damage = damage;
        m_ammoDirection = ammoDirection;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="onHitCallback">击中目标后执行的回调</param>
    public void Launch(Vector3 targetPosition, Action onHitCallback = null)
    {
        m_shadowTran = Instantiate(m_shadowPrefab).transform;
        //参数3：跳跃次数
        transform.DOJump(targetPosition, m_jumpPower, 1, m_duration)
            .SetEase(Ease.Linear)//使用动画曲线
            .OnComplete(() =>
            {
                Destroy(gameObject);

            });
        if (m_enableRotation)
        {
            transform.DORotate(m_rotateAmout, m_duration, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        }
    }
    private void Update()
    {
        if(m_shadowTran != null)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position,
                -Vector2.up, 100, 1 << LayerMask.NameToLayer("GroundReal"));
            if(hitInfo.collider != null)
            {
                m_shadowTran.position = new Vector3(transform.position.x, hitInfo.collider.transform.position.y, 0);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(m_damage, m_ammoDirection);
            Destroy(gameObject);
        }
            
    }
    private void OnDestroy()
    {
        transform.DOKill();
        if(m_shadowTran != null)
            Destroy(m_shadowTran.gameObject);
    }
}
