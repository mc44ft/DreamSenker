using System;
using DreamSeeker.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels.MainMenu.Page
{
    public class SavePage : MonoBehaviour
    {
        [SerializeField] private Button _saveButton;

        private void OnEnable()
        {
            if (_saveButton != null) _saveButton.onClick.AddListener(OnSaveButtonClick);
        }

        private void OnDisable()
        {
            if(_saveButton != null) _saveButton.onClick.RemoveListener(OnSaveButtonClick);
        }

        private void OnSaveButtonClick()
        {
            GameManager.Instance.SaveDataAll();
        }
    }
}