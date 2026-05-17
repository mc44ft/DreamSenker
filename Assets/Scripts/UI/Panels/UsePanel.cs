using DreamSeeker.Inventory;
using DreamSeeker.UI.Panels.MainMenu;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DreamSeeker.UI.Panels
{
    public class UsePanel : PanelBase_Mini
    {
        [SerializeField] private Button _sureButton;
        [SerializeField] private Button _closeButton;
        
        //当前使用的物品信息
        private InventoryItemStack _stack;
        
        public override void OnShowFadePreComplete()
        {
            RemoveButtonListeners();
            AddButtonListeners();
        }

        public override void OnHideFadedComplete()
        {
            RemoveButtonListeners();
        }

        /// <summary>
        /// 绑定确认使用面板按钮监听。
        /// </summary>
        private void AddButtonListeners()
        {
            _sureButton.onClick.AddListener(OnSureButtonClick);
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        /// <summary>
        /// 移除确认使用面板按钮监听。
        /// </summary>
        private void RemoveButtonListeners()
        {
            _sureButton.onClick.RemoveListener(OnSureButtonClick);
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        public void SetUp(InventoryItemStack stack)
        {
            _stack = stack;
        }

        private void OnSureButtonClick()
        {
            if (_stack.ItemInfo == null)
            {
                return;
            }
            if (!InventoryManager.Instance.TryUseItem(_stack.ItemType))
            {
                Debug.LogWarning("物品使用失败");
                //此处效果后续再考虑
            }
            UIManager.Instance.GetPanel<MainMenuPanel>((panel) =>
            {
                panel.PackagePage?.Refresh();
            });
            UIManager.Instance.HidePanel<UsePanel>();
        }

        private void OnCloseButtonClick()
        {
            UIManager.Instance.HidePanel<UsePanel>();
        }
    }
}
