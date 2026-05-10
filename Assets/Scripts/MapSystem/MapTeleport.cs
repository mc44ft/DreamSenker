using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamSenker.Managers;
using DreamSenker.MapSystem.Data;
using DreamSenker.Shared;

/// <summary>
/// 这是一个特殊的任意门触发器脚本
/// 用于传送特定地图
/// </summary>

namespace DreamSenker.MapSystem
{
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class MapTeleport : MonoBehaviour
{
    [SerializeField] private MapDefinitionSO _targetMap;
    [Tooltip("可指定目标传送点位的ID")]
    [SerializeField] private string _teleportPointID;
    [Tooltip("Loading图")]
    [SerializeField] private Sprite _loadingSprite;
    [Tooltip("Loading时间")]
    [SerializeField] private float _loadingTime = 4f;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            if (_targetMap == null)
            {
                Debug.LogError($"{name} 的目标地图未配置");
                return;
            }

            SceneTransition.Instance.ResetLoading(_loadingSprite, _loadingTime);
            GameManager.Instance.TeleportMap(_targetMap.MapId, _teleportPointID);
        }
    }
}
}
