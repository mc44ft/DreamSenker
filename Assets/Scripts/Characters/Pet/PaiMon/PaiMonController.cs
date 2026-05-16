using DG.Tweening;
using UnityEngine;

using DreamSeeker.Characters.Player;
using DreamSeeker.Managers;

namespace DreamSeeker.Characters.Pet
{
public class PaiMonController : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField] private Vector3 _followOffset = new Vector3(-1.2f, 1.1f, 0f);//相对 CameraFollowTran 的跟随偏移
    [SerializeField] private float _followDuration = 0.35f;//飞向目标点的缓动时间
    [SerializeField] private float _retargetDistance = 0.05f;//目标点变化超过该距离才重定向，避免每帧重建 Tween
    [SerializeField] private Ease _followEase = Ease.OutSine;//跟随飞行动画曲线
    [SerializeField] private bool _flipAfterMove = true;//是否在移动到另一侧后再瞬间翻转朝向

    [Header("Float")]
    [SerializeField] private float _floatingHeight = 0.18f;//上下浮动高度
    [SerializeField] private float _floatingLoopDuration = 1.1f;//单次上浮或下落时间
    [SerializeField] private Ease _floatingEase = Ease.InOutSine;//浮动动画曲线

    private static PaiMonController _spawnedInstance;//当前临时宠物实例，防止重复生成

    private Transform _followTarget;//跟随目标，来自 PlayerController.CameraFollowTran
    private Tweener _followTween;//飞向跟随点的 Tween
    private Tween _floatingTween;//控制浮动偏移的 Tween
    private Vector3 _currentFollowPosition;//当前跟随基础坐标，不包含浮动偏移
    private Vector3 _lastTweenTarget;//上一次提交给跟随 Tween 的基础坐标
    private float _floatingOffsetY;//DOTween 驱动的当前浮动偏移
    private float _lastTargetYRotation;//上一次记录的目标 Y 轴朝向
    private float _pendingTargetYRotation;//等待跟随移动完成后应用的目标 Y 轴朝向
    private bool _hasPendingRotation;//是否有待应用的翻转朝向
    private bool _isInitialized;//是否已经绑定目标

    /// <summary>
    /// 伴随玩家生成 PaiMon；主要给 GameManager 玩家生成流程调用。
    /// </summary>
    public static PaiMonController SpawnForPlayer(PlayerController player)
    {
        if (player == null || player.CameraFollowTran == null)
        {
            return null;
        }

        if (_spawnedInstance != null)
        {
            Destroy(_spawnedInstance.gameObject);
            _spawnedInstance = null;
        }

        GameObject instance = CreateInstance(player.CameraFollowTran.position);
        PaiMonController controller = instance.GetComponent<PaiMonController>();
        if (controller == null)
        {
            controller = instance.AddComponent<PaiMonController>();
        }

        controller.Initialize(player.CameraFollowTran);
        _spawnedInstance = controller;
        return controller;
    }

    /// <summary>
    /// 初始化跟随目标和循环浮动效果。
    /// </summary>
    public void Initialize(Transform followTarget)
    {
        _followTarget = followTarget;
        _isInitialized = _followTarget != null;

        if (!_isInitialized)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
        _currentFollowPosition = GetBaseFollowPosition();
        _lastTweenTarget = _currentFollowPosition;
        _lastTargetYRotation = GetTargetYRotation();
        SnapToYRotation(_lastTargetYRotation);
        ApplyPosition();

        StartFloatingTween();
        RetargetFollowTween(force: true);
    }

    /// <summary>
    /// 自动绑定玩家；用于手动把脚本挂到场景物体时也能运行。
    /// </summary>
    private void Start()
    {
        if (_isInitialized)
        {
            return;
        }

        Initialize(GameManager.Instance != null && GameManager.Instance.Player != null
            ? GameManager.Instance.Player.CameraFollowTran
            : null);
    }

    /// <summary>
    /// 持续把 PaiMon 的目标点重定向到玩家镜头跟随点。
    /// </summary>
    private void Update()
    {
        if (!_isInitialized)
        {
            return;
        }

        if (_followTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        RecordTargetRotationChange();
        RetargetFollowTween(force: false);
    }

    /// <summary>
    /// 清理 DOTween，避免销毁后仍回调 Transform。
    /// </summary>
    private void OnDestroy()
    {
        _followTween?.Kill();
        _floatingTween?.Kill();

        if (_spawnedInstance == this)
        {
            _spawnedInstance = null;
        }
    }

    /// <summary>
    /// 创建宠物实例；优先从 Resources 加载 PaiMon 预制体，找不到时退回普通空物体。
    /// </summary>
    private static GameObject CreateInstance(Vector3 spawnPosition)
    {
        GameObject prefab = Resources.Load<GameObject>("PaiMon");
        if (prefab != null)
        {
            return Instantiate(prefab, spawnPosition, Quaternion.identity);
        }

        return new GameObject("PaiMon");
    }

    /// <summary>
    /// 启动上下浮动偏移 Tween，实际位置仍由跟随 Tween 统一写入。
    /// </summary>
    private void StartFloatingTween()
    {
        _floatingTween?.Kill();
        _floatingOffsetY = 0f;
        _floatingTween = DOTween
            .To(() => _floatingOffsetY, value =>
            {
                _floatingOffsetY = value;
                ApplyPosition();
            }, _floatingHeight, _floatingLoopDuration)
            .SetEase(_floatingEase)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    /// <summary>
    /// 根据目标点变化重定向飞行动画。
    /// </summary>
    private void RetargetFollowTween(bool force)
    {
        Vector3 targetPosition = GetBaseFollowPosition();
        if (!force && (targetPosition - _lastTweenTarget).sqrMagnitude < _retargetDistance * _retargetDistance)
        {
            ApplyPendingRotationIfIdle();
            return;
        }

        _lastTweenTarget = targetPosition;

        if (_followTween == null || !_followTween.IsActive())
        {
            _followTween = DOTween
                .To(() => _currentFollowPosition, value =>
                {
                    _currentFollowPosition = value;
                    ApplyPosition();
                }, targetPosition, _followDuration)
                .SetEase(_followEase)
                .SetAutoKill(false)
                .OnComplete(ApplyPendingRotation)
                .SetLink(gameObject);
            return;
        }

        _followTween.ChangeEndValue(targetPosition, _followDuration, true).Restart();
    }

    /// <summary>
    /// 写入最终坐标，基础跟随位置上叠加浮动偏移。
    /// </summary>
    private void ApplyPosition()
    {
        transform.position = _currentFollowPosition + Vector3.up * _floatingOffsetY;
    }

    /// <summary>
    /// 计算跟随基础坐标；CameraFollowTran 旋转时偏移会跟着翻到另一侧。
    /// </summary>
    private Vector3 GetBaseFollowPosition()
    {
        return _followTarget.TransformPoint(_followOffset);
    }

    /// <summary>
    /// 记录目标点 Y 轴变化；只记待翻转朝向，不立刻改 PaiMon 朝向。
    /// </summary>
    private void RecordTargetRotationChange()
    {
        if (!_flipAfterMove || _followTarget == null)
        {
            return;
        }

        float targetYRotation = GetTargetYRotation();
        if (Mathf.Abs(Mathf.DeltaAngle(_lastTargetYRotation, targetYRotation)) < 1f)
        {
            return;
        }

        _lastTargetYRotation = targetYRotation;
        _pendingTargetYRotation = targetYRotation;
        _hasPendingRotation = true;
    }

    /// <summary>
    /// 跟随移动完成后应用待翻转朝向。
    /// </summary>
    private void ApplyPendingRotation()
    {
        if (!_hasPendingRotation)
        {
            return;
        }

        SnapToYRotation(_pendingTargetYRotation);
        _hasPendingRotation = false;
    }

    /// <summary>
    /// 没有移动 Tween 时直接应用待翻转朝向，防止极小位移卡住。
    /// </summary>
    private void ApplyPendingRotationIfIdle()
    {
        if (_followTween != null && _followTween.IsActive() && _followTween.IsPlaying())
        {
            return;
        }

        ApplyPendingRotation();
    }

    /// <summary>
    /// 读取跟随目标的 Y 轴朝向。
    /// </summary>
    private float GetTargetYRotation()
    {
        return _followTarget.eulerAngles.y;
    }

    /// <summary>
    /// 瞬间设置 PaiMon 的 Y 轴朝向。
    /// </summary>
    private void SnapToYRotation(float yRotation)
    {
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.y = yRotation;
        transform.eulerAngles = eulerAngles;
    }
}
}
