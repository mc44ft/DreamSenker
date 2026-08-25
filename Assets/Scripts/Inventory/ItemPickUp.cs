
using UnityEngine;
using UnityEngine.Serialization;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.Inventory
{
[RequireComponent(typeof(BoxCollider2D))]
public class ItemPickUp : MonoBehaviour
{
    [SerializeField] private string _title;//仅用作简单描述
    [FormerlySerializedAs("_itemID")] [SerializeField] private EPackageItemType itemType;
    [SerializeField] private GameObject _worldTips;

    private bool _isPlayerInside;
    private void Start()
    {
        _worldTips.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            _isPlayerInside = true;
            _worldTips.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag(Settings.PlayerTag))
        {
            _isPlayerInside = false;
            _worldTips.SetActive(false);
        }
    }
    private void Update()
    {
        if (InputManager.Instance.PickupButtonDown && _isPlayerInside)
        {
            // 将拾取物添加到背包数据中
            if (!InventoryManager.Instance.TryAddItem(itemType))
            {
                return;
            }

            AudioManager.Instance.PlaySound(GameResources.Instance.PickupItemClip);

            Destroy(gameObject);
        }
    }
}
}
