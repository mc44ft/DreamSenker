using DG.Tweening;
using System;
using UnityEngine;
[RequireComponent(typeof(CircleCollider2D))]
[DisallowMultipleComponent]
public class SlimeAmmo : MonoBehaviour
{
    [SerializeField] private GameObject _shadowPrefab;

    [Tooltip("抛物线高度")]
    [SerializeField] private float _jumpPower = 5f;
    [Tooltip("飞行时间")]
    [SerializeField] private float _duration = 1.5f;
    [Tooltip("飞行过程中是否旋转")]
    [SerializeField] private bool _enableRotation = true;
    [Tooltip("旋转角度")]
    [SerializeField] private Vector3 _rotateAmout = new Vector3(0, 0, -360);

    //阴影位置
    private Transform _shadowTran;

    //子弹数据
    private int _damage;
    private Vector2 _ammoDirection;


    public void Init(int damage, Vector2 ammoDirection)
    {
        _damage = damage;
        _ammoDirection = ammoDirection;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="onHitCallback">击中目标后执行的回调</param>
    public void Launch(Vector3 targetPosition, Action onHitCallback = null)
    {
        _shadowTran = Instantiate(_shadowPrefab).transform;
        //参数3：跳跃次数
        transform.DOJump(targetPosition, _jumpPower, 1, _duration)
            .SetEase(Ease.Linear)//使用动画曲线
            .OnComplete(() =>
            {
                Destroy(gameObject);

            });
        if (_enableRotation)
        {
            transform.DORotate(_rotateAmout, _duration, RotateMode.FastBeyond360).SetEase(Ease.Linear);
        }
    }
    private void Update()
    {
        if(_shadowTran != null)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(transform.position,
                -Vector2.up, 100, 1 << LayerMask.NameToLayer("GroundReal"));
            if(hitInfo.collider != null)
            {
                _shadowTran.position = new Vector3(transform.position.x, hitInfo.collider.transform.position.y, 0);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>().TakeDamage(_damage, _ammoDirection);
            Destroy(gameObject);
        }
            
    }
    private void OnDestroy()
    {
        transform.DOKill();
        if(_shadowTran != null)
            Destroy(_shadowTran.gameObject);
    }
}
