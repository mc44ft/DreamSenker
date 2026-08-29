using System.Collections;
using TMPro;
using UnityEngine;

namespace DreamSeeker.UI
{
    /// <summary>
    /// Tip popup 的显示位置。
    /// </summary>
    public enum TipPopupPosition
    {
        Top,
        Middle,
        Bottom
    }

    /// <summary>
    /// 短时提示面板，显示 2 秒后自动隐藏。
    /// </summary>
    public class TipPopupPanel : PanelBase_Mini
    {
        /// <summary>
        /// 提示信息文本对象。
        /// </summary>
        [SerializeField] private TextMeshProUGUI _tipText;

        /// <summary>
        /// 面板自身的矩形变换组件。
        /// </summary>
        private RectTransform _rectTransform;
        /// <summary>
        /// 自动隐藏协程句柄。
        /// </summary>
        private Coroutine _hideCoroutine;
        /// <summary>
        /// 当前提示的显示位置。
        /// </summary>
        private TipPopupPosition _position;

        /// <summary>
        /// 缓存面板矩形变换组件。
        /// </summary>
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        /// <summary>
        /// 开启提示淡入淡出效果，并在 2 秒后自动隐藏。
        /// </summary>
        /// <param name="position">提示在 Canvas 上的垂直位置。</param>
        /// <param name="message">要显示的提示信息。</param>
        public static void ShowTip(TipPopupPosition position, string message)
        {
            UIManager.Instance.ShowPanel<TipPopupPanel>(
                E_UILayer.Top,
                panel => panel.SetTip(position, message));
        }

        /// <summary>
        /// 设置提示文本和位置，并重置自动隐藏计时。
        /// </summary>
        private void SetTip(TipPopupPosition position, string message)
        {
            _position = position;
            _tipText.text = message;
            SetPosition();

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
            }

            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }

        /// <summary>
        /// 按父 Canvas 高度的 1/10、5/10、9/10 位置排列提示。
        /// </summary>
        private void SetPosition()
        {
            RectTransform parentRect = _rectTransform.parent as RectTransform;
            if (parentRect == null)
            {
                return;
            }

            float normalizedHeight = _position switch
            {
                TipPopupPosition.Top => 0.4f,
                TipPopupPosition.Bottom => -0.4f,
                _ => 0f
            };
            Vector2 anchoredPosition = _rectTransform.anchoredPosition;
            anchoredPosition.y = parentRect.rect.height * normalizedHeight;
            _rectTransform.anchoredPosition = anchoredPosition;
        }

        /// <summary>
        /// 等待 2 秒后请求 UIManager 使用 Widget 淡出并隐藏面板。
        /// </summary>
        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(2f);
            UIManager.Instance.HidePanel<TipPopupPanel>();
            _hideCoroutine = null;
        }

        /// <summary>
        /// 面板淡入前重新应用提示位置。
        /// </summary>
        public override void OnShowFadePreComplete()
        {
            SetPosition();
        }

        /// <summary>
        /// 面板淡出完成后清空提示文本。
        /// </summary>
        public override void OnHideFadedComplete()
        {
            _tipText.text = string.Empty;
        }
    }
}
