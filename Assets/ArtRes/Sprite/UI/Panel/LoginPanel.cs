using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using DreamSeeker.Data;
using DreamSeeker.Managers;
using DreamSeeker.Shared;

namespace DreamSeeker.UI
{
    /// <summary>
    /// 登录面板：收集账号和密码，并将确认结果交给外部登录流程。
    /// </summary>
    public class LoginPanel : PanelBase_Mini
    {
        [Header("Buttons")]
        [SerializeField] private Button _confirmButton;//确认登录按钮
        [SerializeField] private Button _registerButton;//注册按钮
        [SerializeField] private Button _closeButton;//关闭面板按钮

        [Header("Input Fields")]
        [SerializeField] private TMP_InputField _accountInputField;//账号输入框
        [SerializeField] private TMP_InputField _passwordInputField;//密码输入框

        private Action _onLoginSucceeded;//登录成功回调
        private bool _isLoginRequestInProgress;//登录请求进行中标记，避免重复提交

        /// <summary>
        /// 注入登录成功回调。
        /// </summary>
        public void Initialize(Action onLoginSucceeded)
        {
            _onLoginSucceeded = onLoginSucceeded;
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
            if (_registerButton != null)
                _registerButton.onClick.AddListener(OnRegisterButtonClick);
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
            if (_registerButton != null)
                _registerButton.onClick.RemoveListener(OnRegisterButtonClick);
            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        /// <summary>
        /// 校验输入并提交登录凭据。
        /// </summary>
        private void OnConfirmButtonClick()
        {
            if (_isLoginRequestInProgress)
            {
                return;
            }

            string account = _accountInputField != null ? _accountInputField.text.Trim() : string.Empty;
            string password = _passwordInputField != null ? _passwordInputField.text : string.Empty;

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "登录失败：账号和密码不能为空");
                return;
            }

            AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
            _isLoginRequestInProgress = true;
            StartCoroutine(AuthClient.Instance.Login(account, password, OnLoginSucceeded, OnLoginFailed));
        }

        /// <summary>
        /// 校验输入并提交注册凭据。
        /// </summary>
        private void OnRegisterButtonClick()
        {
            if (_isLoginRequestInProgress)
            {
                return;
            }

            string account = _accountInputField != null ? _accountInputField.text.Trim() : string.Empty;
            string password = _passwordInputField != null ? _passwordInputField.text : string.Empty;

            if (string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "注册失败：账号和密码不能为空");
                return;
            }

            AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);
            _isLoginRequestInProgress = true;
            StartCoroutine(AuthClient.Instance.Register(account, password, OnRegisterSucceeded, OnRegisterFailed));
        }

        /// <summary>
        /// 注册成功后提示用户，并允许再次提交请求。
        /// </summary>
        private void OnRegisterSucceeded()
        {
            _isLoginRequestInProgress = false;
            HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "注册成功，请登录");
        }

        /// <summary>
        /// 注册失败时恢复提交状态并输出服务端错误信息。
        /// </summary>
        private void OnRegisterFailed(string error)
        {
            _isLoginRequestInProgress = false;
            HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "注册失败");
            Debug.LogWarning($"注册失败：{error}");
        }

        /// <summary>
        /// 登录成功后关闭面板，并通知开始菜单继续进入游戏。
        /// </summary>
        private void OnLoginSucceeded(LoginResponse response)
        {
            _isLoginRequestInProgress = false;
            HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "登录成功");
            UIManager.Instance.HidePanel<LoginPanel>(null, () => _onLoginSucceeded?.Invoke());
        }

        /// <summary>
        /// 登录失败时恢复提交状态并输出服务端错误信息。
        /// </summary>
        private void OnLoginFailed(string error)
        {
            _isLoginRequestInProgress = false;
            HelperUtilities.ShowTipPopup(TipPopupPosition.Bottom, "登录失败");
            Debug.LogWarning($"登录失败：{error}");
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
