
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
public class ItemPickUp : MonoBehaviour
{
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
            AudioManager.Instance.PlaySound(GameResources.Instance.PickupItemClip);
            //将自己添加到玩家数据中
            InventoryManager.Instance.AddItemToPackage(itemType);

            //更新游戏数据
            if(itemType == EPackageItemType.Chen)
            {
                GameManager.Instance.GameSaveData.IsGotChen = true;
            }

            Destroy(gameObject);
        }
    }
}
