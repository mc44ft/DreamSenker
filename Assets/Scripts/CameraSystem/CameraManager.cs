using System;
using System.Collections;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

using DreamSeeker.Managers;

namespace DreamSeeker.CameraSystem
{
[DisallowMultipleComponent]
public class CameraManager : SingletonMono<CameraManager>
{
    [Header("VCAM")]
    [SerializeField] private CinemachineVirtualCamera _followCamera;
    [SerializeField] private CinemachineVirtualCamera _dialogueCamera;

    [Header("BOSS")]
    [SerializeField] private float _bossCameraMoveDuration = 1f;
    [SerializeField] private Ease _bossCameraMoveEase = Ease.InOutQuad;

    private Coroutine _bindPlayerRoutine;
    private Tween _bossCameraTween;
    private Transform _playerTarget;
    private bool _hasDefaultDialogueCameraSettings;//是否已缓存对话相机初始配置
    private float _defaultDialogueCameraOrthoSize;//对话相机默认正交视野大小
    private float _defaultDialogueCameraOffsetY;//对话相机默认跟随偏移Y值

    /// <summary>
    /// 检查虚拟相机配置，并把虚拟相机初始化绑定到当前玩家
    /// </summary>
    public void SetupVCams()
    {
        //检查跟随相机是否配置
        if (!ValidateFollowCamera())
        {
            return;
        }

        //初始化时两个虚拟相机都先绑定玩家，对话时再临时切到 NPC
        BindConfiguredCamerasToPlayer();
        
        //缓存对话相机初始配置，用于退出对话时恢复
        CacheDefaultDialogueCameraSettings();

        //初始关闭_dialogueCamera
        if (_dialogueCamera != null)
        {
            _dialogueCamera.gameObject.SetActive(false);
        }
        
        // RefreshFollowCameraAfterPlayerWarp(Vector3.zero, Vector3.zero);
    }

    /// <summary>
    /// 进入对话镜头，让对话虚拟相机跟随指定 NPC
    /// </summary>
    public void EnterDialogue(Transform npcTransform)
    {
        if (npcTransform == null)
        {
            return;
        }

        if (!ValidateDialogueCamera())
        {
            return;
        }

        _dialogueCamera.Follow = npcTransform;
        _dialogueCamera.LookAt = npcTransform;
        ApplyCurrentMapDialogueCameraSettings();
        _dialogueCamera.gameObject.SetActive(true);
    }

    /// <summary>
    /// 退出对话镜头，恢复玩家跟随相机
    /// </summary>
    public void ExitDialogue()
    {
        BindFollowCameraToPlayer();

        if (_dialogueCamera == null)
        {
            return;
        }

        _dialogueCamera.Follow = null;
        _dialogueCamera.LookAt = null;
        RestoreDefaultDialogueCameraSettings();
        _dialogueCamera.gameObject.SetActive(false);
    }

    /// <summary>
    /// 进入 Boss 战镜头，断开玩家跟随并移动到 Boss 房中心
    /// </summary>
    public void EnterBossFight(Vector3 bossCenter, Action onCameraArrived = null)
    {
        if (!ValidateFollowCamera())
        {
            onCameraArrived?.Invoke();
            return;
        }

        _playerTarget = _followCamera.Follow != null ? _followCamera.Follow : GetCurrentPlayerTransform();
        _followCamera.Follow = null;
        _followCamera.LookAt = null;

        //进入 Boss 战时使用当前地图配置的 Boss 镜头视野
        if (GameManager.Instance != null && GameManager.Instance.TryGetCurrentMapBossCameraOrthoSize(out float bossOrthoSize))
        {
            SetFollowCameraOrthoSize(bossOrthoSize);
        }

        Vector3 cameraNewPos = new Vector3(bossCenter.x, bossCenter.y, _followCamera.transform.position.z);

        _bossCameraTween?.Kill(false);
        _bossCameraTween = _followCamera.transform.DOMove(cameraNewPos, _bossCameraMoveDuration)
            .SetEase(_bossCameraMoveEase)
            .OnComplete(() =>
            {
                _bossCameraTween = null;
                onCameraArrived?.Invoke();
            });
    }

    /// <summary>
    /// 退出 Boss 战镜头，取消镜头移动并恢复跟随玩家
    /// </summary>
    public void ExitBossFight()
    {
        _bossCameraTween?.Kill(false);
        _bossCameraTween = null;
        BindFollowCameraToPlayer();

        //退出 Boss 战时恢复当前地图日常跟随视野
        if (GameManager.Instance != null && GameManager.Instance.TryGetCurrentMapFollowCameraOrthoSize(out float followOrthoSize))
        {
            SetFollowCameraOrthoSize(followOrthoSize);
        }
    }

    /// <summary>
    /// 设置跟随虚拟相机的正交视野大小
    /// </summary>
    public void SetFollowCameraOrthoSize(float orthoSize)
    {
        if (!ValidateFollowCamera())
        {
            return;
        }

        _followCamera.m_Lens.OrthographicSize = orthoSize;
    }

    /// <summary>
    /// 将指定物体上的 2D 碰撞体设置为 Follow 虚拟相机的 Confiner2D 边界。
    /// </summary>
    public bool SetFollowCameraConfinerBounds(GameObject boundsObject)
    {
        if (!ValidateFollowCamera())
        {
            return false;
        }

        CinemachineConfiner2D confiner = GetOrAddFollowCameraConfiner();
        if (confiner == null)
        {
            return false;
        }

        if (boundsObject == null)
        {
            //非地图场景没有 Wall 时清空旧边界，避免沿用上一张地图的相机限制
            confiner.m_BoundingShape2D = null;
            confiner.InvalidateCache();
            return false;
        }

        CompositeCollider2D boundsCollider = boundsObject.GetComponent<CompositeCollider2D>();
        if (boundsCollider == null)
        {
            Debug.LogError($"{boundsObject.name} 缺少用于 Follow Camera Confiner2D 的 Collider2D", boundsObject);
            return false;
        }

        confiner.m_BoundingShape2D = boundsCollider;
        confiner.InvalidateCache();
        return true;
    }

    /// <summary>
    /// 玩家瞬移后刷新跟随相机状态，避免 Cinemachine 沿用旧场景缓存产生偏移
    /// </summary>
    public void RefreshFollowCameraAfterPlayerWarp(Vector3 previousPosition, Vector3 currentPosition)
    {
        if (!ValidateFollowCamera())
        {
            return;
        }

        Transform playerTransform = GetCurrentPlayerTransform();
        if (playerTransform == null)
        {
            return;
        }

        _playerTarget = playerTransform;
        _followCamera.Follow = _playerTarget;

        //通知 Cinemachine 跟随目标发生瞬移，并立刻取消本帧阻尼缓存
        Vector3 positionDelta = currentPosition - previousPosition;
        //立即刷新相机位置
        _followCamera.OnTargetObjectWarped(_playerTarget, positionDelta);
        //取消当前帧的阻尼缓动 直接跑到目标位置
        _followCamera.CancelDamping(true);
    }

    /// <summary>
    /// 检查主跟随相机是否已在 Inspector 配置
    /// </summary>
    private bool ValidateFollowCamera()
    {
        if (_followCamera != null)
        {
            return true;
        }

        Debug.LogError($"{nameof(CameraManager)} 未配置 Follow Camera", this);
        return false;
    }

    /// <summary>
    /// 检查对话相机是否已在 Inspector 配置
    /// </summary>
    private bool ValidateDialogueCamera()
    {
        if (_dialogueCamera != null)
        {
            return true;
        }

        Debug.LogError($"{nameof(CameraManager)} 未配置 Dialogue Camera", this);
        return false;
    }

    /// <summary>
    /// 缓存对话虚拟相机初始正交视野和Y轴偏移
    /// </summary>
    private void CacheDefaultDialogueCameraSettings()
    {
        if (_hasDefaultDialogueCameraSettings || _dialogueCamera == null)
        {
            return;
        }

        _defaultDialogueCameraOrthoSize = _dialogueCamera.m_Lens.OrthographicSize;
        _defaultDialogueCameraOffsetY = GetDialogueCameraOffsetY();
        _hasDefaultDialogueCameraSettings = true;
    }

    /// <summary>
    /// 根据当前地图配置应用对话相机正交视野和Y轴偏移
    /// </summary>
    private void ApplyCurrentMapDialogueCameraSettings()
    {
        if (GameManager.Instance == null || !GameManager.Instance.TryGetCurrentMapDialogueCameraSettings(out float orthoSize, out float offsetY))
        {
            return;
        }

        SetDialogueCameraOrthoSize(orthoSize);
        SetDialogueCameraOffsetY(offsetY);
    }

    /// <summary>
    /// 退出对话时恢复对话相机初始配置，避免跨地图残留
    /// </summary>
    private void RestoreDefaultDialogueCameraSettings()
    {
        if (!_hasDefaultDialogueCameraSettings)
        {
            return;
        }

        SetDialogueCameraOrthoSize(_defaultDialogueCameraOrthoSize);
        SetDialogueCameraOffsetY(_defaultDialogueCameraOffsetY);
    }

    /// <summary>
    /// 设置对话虚拟相机正交视野大小
    /// </summary>
    private void SetDialogueCameraOrthoSize(float orthoSize)
    {
        if (_dialogueCamera == null)
        {
            return;
        }

        _dialogueCamera.m_Lens.OrthographicSize = orthoSize;
    }

    /// <summary>
    /// 设置对话虚拟相机 FramingTransposer 的Y轴偏移
    /// </summary>
    private void SetDialogueCameraOffsetY(float offsetY)
    {
        CinemachineFramingTransposer framingTransposer = GetDialogueCameraFramingTransposer();
        if (framingTransposer == null)
        {
            return;
        }

        Vector3 trackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        trackedObjectOffset.y = offsetY;
        framingTransposer.m_TrackedObjectOffset = trackedObjectOffset;
    }

    /// <summary>
    /// 获取对话虚拟相机 FramingTransposer 的Y轴偏移
    /// </summary>
    private float GetDialogueCameraOffsetY()
    {
        CinemachineFramingTransposer framingTransposer = GetDialogueCameraFramingTransposer();
        return framingTransposer != null ? framingTransposer.m_TrackedObjectOffset.y : 0f;
    }

    /// <summary>
    /// 获取对话虚拟相机的 FramingTransposer 组件
    /// </summary>
    private CinemachineFramingTransposer GetDialogueCameraFramingTransposer()
    {
        return _dialogueCamera != null ? _dialogueCamera.GetCinemachineComponent<CinemachineFramingTransposer>() : null;
    }

    /// <summary>
    /// 获取或创建 Follow 虚拟相机上的 Confiner2D 扩展。
    /// </summary>
    private CinemachineConfiner2D GetOrAddFollowCameraConfiner()
    {
        if (_followCamera == null)
        {
            return null;
        }

        CinemachineConfiner2D confiner = _followCamera.GetComponent<CinemachineConfiner2D>();
        if (confiner == null)
        {
            confiner = _followCamera.gameObject.AddComponent<CinemachineConfiner2D>();
        }

        return confiner;
    }

    /// <summary>
    /// 将主跟随相机绑定到当前玩家；玩家还没生成时启动等待协程
    /// </summary>
    private void BindFollowCameraToPlayer()
    {
        if (_followCamera == null)
        {
            return;
        }

        Transform playerTransform = GetCurrentPlayerTransform();
        if (playerTransform == null)
        {
            StartBindPlayerRoutine();
            return;
        }

        _playerTarget = playerTransform;
        _followCamera.Follow = _playerTarget;
    }

    /// <summary>
    /// 将已配置的虚拟相机初始化绑定到当前玩家；玩家还没生成时启动等待协程
    /// </summary>
    private void BindConfiguredCamerasToPlayer()
    {
        Transform playerTransform = GetCurrentPlayerTransform();
        if (playerTransform == null)
        {
            StartBindPlayerRoutine();
            return;
        }

        _playerTarget = playerTransform;
        BindCameraFollow(_followCamera, _playerTarget);
        BindCameraFollow(_dialogueCamera, _playerTarget);
    }

    /// <summary>
    /// 将指定虚拟相机的 Follow 目标绑定到玩家
    /// </summary>
    private void BindCameraFollow(CinemachineVirtualCamera virtualCamera, Transform playerTransform)
    {
        if (virtualCamera == null || playerTransform == null)
        {
            return;
        }

        virtualCamera.Follow = playerTransform;
    }

    /// <summary>
    /// 获取当前玩家 Transform；没有玩家时返回上一次缓存目标
    /// </summary>
    private Transform GetCurrentPlayerTransform()
    {
        if (GameManager.Instance != null && 
            GameManager.Instance.Player != null && 
            GameManager.Instance.Player.CameraFollowTran != null)
        {
            return GameManager.Instance.Player.CameraFollowTran.transform;
        }

        return _playerTarget;
    }

    /// <summary>
    /// 启动等待玩家生成的绑定协程
    /// </summary>
    private void StartBindPlayerRoutine()
    {
        if (_bindPlayerRoutine != null)
        {
            return;
        }

        _bindPlayerRoutine = StartCoroutine(BindPlayerRoutine());
    }

    /// <summary>
    /// 等待 GameManager 生成玩家后重新绑定跟随相机
    /// </summary>
    private IEnumerator BindPlayerRoutine()
    {
        while (GameManager.Instance == null || 
               GameManager.Instance.Player == null ||  
               GameManager.Instance.Player.CameraFollowTran == null)
        {
            //每0.5s查找一次
            yield return new WaitForSeconds(0.5f);
        }

        _bindPlayerRoutine = null;
        BindConfiguredCamerasToPlayer();
    }
}
}
