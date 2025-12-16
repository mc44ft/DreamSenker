using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 这是一个特殊的任意门触发器脚本
/// 用于传送特定地图
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class MapTeleport : MonoBehaviour
{
    [SerializeField] private E_MapSceneName _mapSceneName;
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
            SceneTransition.Instance.ResetLoading(_loadingSprite, _loadingTime);
            GameManager.Instance.TeleportMap(GameManager.Instance.GetSceneNameFromEnum(_mapSceneName), _teleportPointID);
        }
    }
}
