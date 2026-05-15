using UnityEngine;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

//挂载出口上 只有挂载了该脚本的Point才是可触发的出口

namespace DreamSeeker.MapSystem
{
[RequireComponent(typeof(BoxCollider2D), typeof(MapLinkPoint))]
[DisallowMultipleComponent]
public class MapTransitionTrigger : MonoBehaviour
{
    [Tooltip("Loading图")]
    [SerializeField] private Sprite _loadingSprite;
    [Tooltip("Loading时间")]
    [SerializeField] private float _loadingTime = 0.5f;

    private BoxCollider2D _boxCollider2D;
    private MapLinkPoint _mapLinkPoint;

    private void Awake()
    {
        _boxCollider2D = GetComponent<BoxCollider2D>();
        _boxCollider2D.isTrigger = true;
        _mapLinkPoint = GetComponent<MapLinkPoint>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            return;
        }

        SceneTransition.Instance.ResetLoading(_loadingSprite, _loadingTime);
        GameManager.Instance.ChangeMap(_mapLinkPoint.PointGuid);
    }
}
}
