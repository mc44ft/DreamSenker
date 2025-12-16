using System.Collections.Generic;
using UnityEngine;
namespace DialogueSystem.UI
{
    [RequireComponent(typeof(Widget))]
    [DisallowMultipleComponent]
    public class ChoicesSection : MonoBehaviour
    {
        private List<DialogueButton> _butonList = new List<DialogueButton>();
        private Widget _widget;
        private void Awake()
        {
            _widget = GetComponent<Widget>();
        }
        public void Init(Data.DialogueNodeChoice.ChoiceData[] datas)
        {
            for (int i = 0; i < datas.Length; i++)
            {
                DialogueButtonCenter buttonCenter = PoolManager.Instance.Pull<DialogueButtonCenter>(datas[i].ButtonPrefab);
                buttonCenter.transform.SetParent(transform, false);
                buttonCenter.transform.localScale = Vector3.one;

                DialogueButton button = buttonCenter.GetComponent<DialogueButton>();
                button.Init(datas[i].Content, i);

                _butonList.Add(button);
            }
        }
        public void Show(int defaultSelectIndex, float duration = 0.2f)
        {
            if (_butonList.Count < 0)
            {
                Debug.LogWarning("选项框中无任何子选项！");
                return;
            }

            _widget.Fade(0f, 0f);

            _widget.Fade(1f, duration, () =>
            {
                if (defaultSelectIndex >= 0 && defaultSelectIndex < _butonList.Count)
                {
                    _butonList[defaultSelectIndex].Select();
                }
                else
                {
                    _butonList[0].Select();
                }
            });
        }
        public void Hide(float duration = 0.2f)
        {
            DialogueManager.Instance.SetCurrentSelectButton(null);

            _widget.Fade(0f, duration, () =>
            {
                foreach (var button in _butonList)
                {
                    button.GetComponent<DialogueButtonCenter>().PushSelfToPool();
                }
            });
        }
    }
}

