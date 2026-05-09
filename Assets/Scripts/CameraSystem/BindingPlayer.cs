using Cinemachine;
using System.Collections;
using UnityEngine;
using DreamSenker.Managers;

/// <summary>
/// 此脚本配置在虚拟相机上 用于绑定玩家
/// </summary>

namespace DreamSenker.CameraSystem
{
[RequireComponent(typeof(CinemachineVirtualCamera))]
[DisallowMultipleComponent]
public class BindingPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera m_virtualCamera;
    private void Awake()
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
    }
    private void Start()
    {
        if(m_virtualCamera.Follow == null)
            StartCoroutine(FindPlayerRoutine());
    }
    private IEnumerator FindPlayerRoutine()
    {
        while(GameManager.Instance.Player == null)
        {
            yield return null;
        }
        m_virtualCamera.Follow = GameManager.Instance.Player.transform;
    }
}
}
