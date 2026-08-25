using TMPro;
using UnityEngine;
namespace PlayArk.DialogueSystem.Runtime
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class Ruby : PoolBase
    {
        private TextMeshProUGUI _text;
        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="color">被注音文字的颜色 确保和被注音文字的颜色显示一致</param>
        public void Init(string content, Color color, Transform parent, Vector3 localPosition)
        {
            _text.SetText(content);
            _text.color = color;
            transform.SetParent(parent);
            transform.localPosition = localPosition;

            transform.localScale = Vector3.one;
        }
        public override void OnPull()
        {

        }

        public override void OnPush()
        {

        }
    }
}

