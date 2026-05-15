using PlayArk.StateMachine.Utilities;
using UnityEngine;


using DreamSeeker.Characters;

/// <summary>
/// OldMan的AI感知组件
/// 负责玩家感知、移动方向计算和状态机谓词判断
/// </summary>

namespace DreamSeeker.Characters.Enemies
{
public class OldManBrain : MonoBehaviour, IPredicateEvaluator
{
    private IEnemyAIConfig _aiConfig;
    private Transform _playerTransform;
    private bool _playerFound;
    private float _moveDirection;

    /// <summary>
    /// 移动方向，-1向左，1向右，0静止
    /// </summary>
    public float MoveDirection => _moveDirection;

    /// <summary>
    /// 注入AI配置
    /// </summary>
    public void InjectionConfig(IEnemyAIConfig config)
    {
        _aiConfig = config;
    }

    private void Update()
    {
        CalculateMoveDirection();
    }

    /// <summary>
    /// 计算移动方向
    /// </summary>
    private void CalculateMoveDirection()
    {
        if (_playerTransform == null)
        {
            _moveDirection = 0;
            return;
        }

        float distanceX = _playerTransform.position.x - transform.position.x;
        _moveDirection = Mathf.Abs(distanceX) > 0.1f ? Mathf.Sign(distanceX) : 0;
    }

    /// <summary>
    /// 获取到玩家的距离，懒加载玩家引用
    /// </summary>
    private float GetDistanceToPlayer()
    {
        // 懒加载玩家引用
        if (!_playerFound)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
                _playerFound = true;
            }
            else
            {
                return float.MaxValue;
            }
        }

        return Vector2.Distance(transform.position, _playerTransform.position);
    }

    public bool? Evaluate(EPredicate predicate, string[] parameters)
    {
        if (_aiConfig == null) return null;

        float distance = GetDistanceToPlayer();

        switch (predicate)
        {
            case EPredicate.CanSeeTarget:
                // 玩家在检测范围内
                return distance <= _aiConfig.DetectRange;

            case EPredicate.InAttackRange:
                // 玩家在攻击范围内
                return distance <= _aiConfig.AttackRange;

            case EPredicate.LostTarget:
                // 玩家超出丢失范围
                return distance > _aiConfig.LoseTargetRange;
        }

        return null;
    }
}
}
