using System;
using UnityEngine;

using DreamSeeker.Characters.Player;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters
{
    /// <summary>
    /// 友好模式下的简单巡逻和待机 / 行走动画控制。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Mover))]
    public class FriendlyPatrol : MonoBehaviour
    {
        private enum FriendlyPatrolState
        {
            Idle,
            Walk
        }

        [Header("Patrol")]
        [Tooltip("友好巡逻点，至少两个点时才会来回移动")]
        [SerializeField] private Transform[] _patrolPoints;
        [Tooltip("到达巡逻点的距离判定")]
        [SerializeField] private float _arriveDistance = 0.1f;
        [Tooltip("到达巡逻点后的停留时间")]
        [SerializeField] private float _waitTime = 1f;
        [Tooltip("玩家进入该范围时停止巡逻")]
        [SerializeField] private float _playerStayRange = 2f;

        [Header("Animation")]
        [Tooltip("友好待机动画状态名")]
        [SerializeField] private string _idleAnimationName = "Idle";
        [Tooltip("友好行走动画状态名")]
        [SerializeField] private string _walkAnimationName = "Walk";
        [Tooltip("友好模式复用的 Animator")]
        [SerializeField] private Animator _animator;

        private Mover _mover;
        private Transform _playerTransform;
        private FriendlyPatrolState _currentState = FriendlyPatrolState.Idle;
        private int _currentPointIndex;
        private int _direction = 1;
        private float _waitTimer;

        private void Awake()
        {
            // 自动缓存组件，Inspector 仍可手动指定 Animator。
            _mover = GetComponent<Mover>();
            _animator ??= GetComponent<Animator>();
        }

        private void OnEnable()
        {
            // 重新启用友好巡逻时，先按当前状态播放一次动画。
            _waitTimer = 0f;
            PlayStateAnimation(_currentState, true);
        }

        private void OnDisable()
        {
            // 关闭友好巡逻后不再操作 Animator，只停止残留移动。
            _mover?.StopMove();
        }

        private void Update()
        {
            if (ShouldStopForPlayer())
            {
                StopAndIdle();
                return;
            }

            Patrol();
        }

        /// <summary>
        /// 执行友好巡逻移动。
        /// </summary>
        private void Patrol()
        {
            if (_patrolPoints == null || _patrolPoints.Length < 2)
            {
                StopAndIdle();
                return;
            }

            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.deltaTime;
                StopAndIdle();
                return;
            }

            Transform targetPoint = _patrolPoints[_currentPointIndex];
            if (targetPoint == null)
            {
                MoveToNextPoint();
                return;
            }

            float distanceX = targetPoint.position.x - transform.position.x;
            if (Mathf.Abs(distanceX) <= _arriveDistance)
            {
                _waitTimer = _waitTime;
                MoveToNextPoint();
                StopAndIdle();
                return;
            }

            _mover.Move(Mathf.Sign(distanceX));
            PlayStateAnimation(FriendlyPatrolState.Walk);
        }

        /// <summary>
        /// 切换到下一个巡逻点，末端自动折返。
        /// </summary>
        private void MoveToNextPoint()
        {
            if (_patrolPoints == null || _patrolPoints.Length < 2)
            {
                return;
            }

            if (_currentPointIndex >= _patrolPoints.Length - 1)
            {
                _direction = -1;
            }
            else if (_currentPointIndex <= 0)
            {
                _direction = 1;
            }

            _currentPointIndex = Mathf.Clamp(_currentPointIndex + _direction, 0, _patrolPoints.Length - 1);
        }

        /// <summary>
        /// 停止移动并切换为友好待机动画。
        /// </summary>
        private void StopAndIdle()
        {
            _mover.StopMove();
            PlayStateAnimation(FriendlyPatrolState.Idle);
        }

        /// <summary>
        /// 判断玩家是否进入友好停留范围。
        /// </summary>
        private bool ShouldStopForPlayer()
        {
            Transform playerTransform = GetPlayerTransform();
            if (playerTransform == null)
            {
                return false;
            }

            return Vector2.Distance(transform.position, playerTransform.position) <= _playerStayRange;
        }

        /// <summary>
        /// 懒加载玩家 Transform。
        /// </summary>
        private Transform GetPlayerTransform()
        {
            if (_playerTransform != null)
            {
                return _playerTransform;
            }

            GameObject player = GameObject.FindWithTag(Settings.PlayerTag);
            _playerTransform = player != null ? player.transform : null;
            return _playerTransform;
        }

        /// <summary>
        /// 巡逻状态变化时播放一次对应动画。
        /// </summary>
        private void PlayStateAnimation(FriendlyPatrolState state, bool force = false)
        {
            if (!force && _currentState == state)
            {
                return;
            }

            _currentState = state;

            if (_animator == null)
            {
                return;
            }

            string animationName = state == FriendlyPatrolState.Walk
                ? _walkAnimationName
                : _idleAnimationName;

            if (!string.IsNullOrEmpty(animationName))
            {
                _animator.Play(animationName);
            }
        }

        private void OnDrawGizmosSelected()
        {
            //绘制玩家监测圈
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _playerStayRange);
        }
    }
}
