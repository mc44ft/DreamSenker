using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Health))]
public class SpiderSilk : MonoBehaviour, IDamageable
{

    private Vector3 _targetPosition;
    private float _silkContinueTime;
    private float _silkSpeed;
    private float _silkClampSpeedRate;


    private bool _isWebSkill;
    private PlayerController _playerController;

    private Rigidbody2D _rb;
    private Health _health;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
    }
    public void Init(Vector3 targetPosition, float silkContinueTime, float silkSpeed, float silkClampSpeedRate, int silkHealthAmount)
    {
        _targetPosition = targetPosition;
        _silkContinueTime = silkContinueTime;
        _silkSpeed = silkSpeed;
        _silkClampSpeedRate = silkClampSpeedRate;

        _health.Initialize(silkHealthAmount, silkHealthAmount);
    }
    private void FixedUpdate()
    {
        if (!_isWebSkill)
        {
            Vector2 unitVector = (_targetPosition - transform.position).normalized;
            _rb.MovePosition(_rb.position + unitVector * _silkSpeed * Time.fixedDeltaTime);

            if (Vector3.Distance(_rb.position, _targetPosition) < 0.1f)
            {
                _isWebSkill = true;
                Destroy(gameObject, _silkContinueTime);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            //限制玩家速度
            _playerController = collision.GetComponent<PlayerController>();
            _playerController.Mover.SetMoveSpeedMultiplier(_silkClampSpeedRate);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            if (_playerController != null)
            {
                //恢复玩家速度
                _playerController.Mover.SetMoveSpeedMultiplier(1f);
            }
        }
    }
    private void OnDestroy()
    {
        if(_playerController != null)
        {
            //恢复玩家速度
            _playerController.Mover.SetMoveSpeedMultiplier(1f);
        }
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        _health.ApplyDamage(1);

        if (_health.IsDead)
        {
            Destroy(gameObject);
        }
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void Select()
    {
        
    }

    public void Deselect()
    {
        
    }
}
