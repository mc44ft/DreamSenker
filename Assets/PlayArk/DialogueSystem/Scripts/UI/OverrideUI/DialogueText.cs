using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace PlayArk.DialogueSystem.Runtime
{
    [RequireComponent(typeof(Widget))]
    public class DialogueText : TextMeshProUGUI
    {
        public const float DefaultFadingDuration = 0.2f;

        public bool IsPrintShowed {  get; private set; }
        /// <summary>
        /// 当前打印下标
        /// </summary>
        private int _currentTypingIndex;
        /// <summary>
        /// 默认逐字打印的间隔时间
        /// </summary>
        private float _defaultIntervalTime = 0.1f;

        private Coroutine _typingCoroutine;
        private Widget _widget;
        /// <summary>
        /// 正在使用的Ruby对象
        /// </summary>
        private List<Ruby> _rubyListUsed = new List<Ruby>();
        
        private DialogueTextPreprocessor SelfTextPreprocessor => (DialogueTextPreprocessor)textPreprocessor;
        protected override void Awake()
        {
            base.Awake();
            _widget = GetComponent<Widget>();
            textPreprocessor = new DialogueTextPreprocessor();
        }
        /// <summary>
        /// 开始打印
        /// </summary>
        public void PrintText(string content, E_DisplayType displayType, float fadingDuration = DefaultFadingDuration)
        {
            IsPrintShowed = false;

            StartCoroutine(ShowTextCoroutine(content, displayType, fadingDuration));
        }
        public void Disappear(Action OnFinished = null, float fadingDuration = DefaultFadingDuration)
        {
            _widget.Fade(0f, fadingDuration, () =>
            {
                ResetSelf();
                OnFinished?.Invoke();
            });
        }
        public void QuickShowRemaining()
        {
            if(_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;

                //将剩下的所有没显示的字全都显示出来
                while(_currentTypingIndex < m_characterCount)
                {
                    StartCoroutine(FadeInCharacterToOut(_currentTypingIndex));
                    _currentTypingIndex++;
                }
                OnPrintShowed();

            }
        }
        private void ResetSelf()
        {
            SetText("");
            ClearRubyAll();
        }
        private IEnumerator ShowTextCoroutine(string content, E_DisplayType displayType, float fadingDuration)
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
            //预处理在此处执行
            SetText(content);

            //先隐藏文字再等待一帧 避免文字闪烁
            //强制更新网格信息 才能读取到设置的文本
            ForceMeshUpdate();
            //单独将每个字的透明度设置为0 再逐字恢复 表现出逐字打印的效果
            SetAllCharacterAlpha(0);

            //等待一帧 等待预处理完成
            yield return null;
            switch (displayType)
            {
                case E_DisplayType.Default:
                    SetAllCharacterAlpha(255);
                    _widget.Fade(1f, 0f, () =>
                    {
                        SetRubyAll(SelfTextPreprocessor.RubyDataList);
                        OnPrintShowed();
                    });
                    break;
                case E_DisplayType.Fading:
                    SetAllCharacterAlpha(255);
                    _widget.Fade(1f, fadingDuration, () =>
                    {
                        SetRubyAll(SelfTextPreprocessor.RubyDataList);
                        OnPrintShowed();
                    });
                    
                    break;
                case E_DisplayType.Typing:
                    _widget.Fade(1f, fadingDuration);
                    _typingCoroutine = StartCoroutine(TypingCoroutine(fadingDuration));
                    break;
            }
        }
        private IEnumerator TypingCoroutine(float fadingDuration)
        {
            _currentTypingIndex = 0;
            while (_currentTypingIndex < m_characterCount)
            {
                StartCoroutine(FadeInCharacterToOut(_currentTypingIndex));

                if (SelfTextPreprocessor.IntervalDict.TryGetValue(_currentTypingIndex, out float intervalTime))
                    yield return new WaitForSecondsRealtime(intervalTime);
                else
                    yield return new WaitForSecondsRealtime(_defaultIntervalTime);
                _currentTypingIndex++;
            }
            OnPrintShowed();
        }
        /// <summary>
        /// 单个字的透明度过渡
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeInCharacterToOut(int index, float duration = DefaultFadingDuration)
        {
            if(SelfTextPreprocessor.TryGetRubyFromStart(index, out RubyData data))
            {
                SetRuby(data);
            }
            if (duration <= 0f)
            {
                SetSingleCharacterAlpha(index, 255);
            }
            float timer = 0f;
            while (timer < duration)
            {
                timer = Mathf.Min(timer + Time.unscaledDeltaTime, duration);

                SetSingleCharacterAlpha(index, (byte)((timer / duration) * 255));
                yield return null;
            }
        }
        private void SetAllCharacterAlpha(byte alpha)
        {
            for (int i = 0; i < m_characterCount; i++)
            {
                SetSingleCharacterAlpha(i, alpha);
            }
        }
        private void SetSingleCharacterAlpha(int index, byte alpha)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[index];

            //排除 空格 等不可见字符（不可见字符无顶点信息，更改其顶点信息会导致第一个字符闪烁Bug）
            if (!characterInfo.isVisible) return;
            //该字符的材质索引
            int materialIndex = characterInfo.materialReferenceIndex;
            //该字符的顶点索引
            int vertexIndex = characterInfo.vertexIndex;
            //一个字符对应四个顶点
            for (int i = 0; i < 4; i++)
            {
                textInfo.meshInfo[materialIndex].colors32[vertexIndex + i].a = alpha;
            }
            UpdateVertexData();
        }
        #region Ruby
        private void SetRubyAll(List<RubyData> dataList)
        {
            foreach (RubyData data in dataList)
            {
                SetRuby(data);
            }
        }
        private void ClearRubyAll()
        {

            foreach (Ruby ruby in _rubyListUsed)
            {
                ruby.PushSelfToPool();
            }
            _rubyListUsed.Clear();
        }
        private void SetRuby(RubyData data)
        {
            Ruby ruby = PoolManager.Instance.Pull<Ruby>("RubyText");
            //设置为自身子物体 随自身一起显隐
            ruby.Init(data.RubyContent, textInfo.characterInfo[data.StartIndex].color, transform,
                (textInfo.characterInfo[data.StartIndex].topLeft + textInfo.characterInfo[data.EndIndex].topRight) / 2);



            _rubyListUsed.Add(ruby);

        }
        #endregion
        private void OnPrintShowed()
        {
            IsPrintShowed = true;
            EventCenter.Instance.EventTrigger(EEventType.Dialogue_PrintShowed, this, new EmptyEventArgs());
        }
        public enum E_DisplayType
        {
            Default,
            Fading,
            Typing
        }
    }
}

