using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Managers;

namespace DreamSeeker.UI
{
    /// <summary>
    /// 登录面板：收集账号和密码，并将确认结果交给外部登录流程。
    /// </summary>
    public class LoginPanel : PanelBase_Mini
    {
        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;//确认登录按钮
        [SerializeField] private Button _closeButton;//关闭面板按钮

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _accountInputField;//账号输入框
        [SerializeField] private TMP_InputField _passwordInputField;//密码输入框

        private Action<string, string> _onLoginConfirmed;//登录确认回调

        /// <summary>
        /// 注入登录确认回调。
        /// </summary>
        public void Initialize(Action<string, string> onLoginConfirmed)
        {
            _onLoginConfirmed = onLoginConfirmed;
        }

        /// <summary>
        /// 面板显示前绑定按钮事件。
        /// </summary>
        public override void OnShowFadePreComplete()
        {
            RemoveButtonListeners();
            AddButtonListeners();
        }

        /// <summary>
        /// 面板隐藏完成后移除按钮事件。
        /// </summary>
        public override void OnHideFadedComplete()
        {
            RemoveButtonListeners();
        }

        /// <summary>
        /// 绑定登录面板按钮监听。
        /// </summary>
        private void AddButtonListeners()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmButtonClick);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        /// <summary>
        /// 移除登录面板按钮监听，避免面板重复显示时重复触发。
        /// </summary>
        private void RemoveButtonListeners()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.RemoveListener(OnConfirmButtonClick);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// 校验输入并提交登录凭据。
        /// </summary>
        private void OnConfirmButtonClick()
        {
            string account = _accountInputField != null ? _accountInputField.text.Trim() : string.Empty;
            string password = _passwordInputField != null ? _passwordInputField.text : string.Empty;

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("登录失败：账号和密码不能为空");
                return;
            }

            AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
            _onLoginConfirmed?.Invoke(account, password);
        }

        /// <summary>
        /// 关闭登录面板。
        /// </summary>
        private void OnCloseButtonClick()
        {
            AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
            UIManager.Instance.HidePanel<LoginPanel>();
        }
    }
}
