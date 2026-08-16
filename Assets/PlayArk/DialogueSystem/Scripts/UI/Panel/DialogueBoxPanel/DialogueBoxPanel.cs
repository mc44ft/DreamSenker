using DialogueSystem;
using PlayArk.DialogueSystem.Data;
using PlayArk.DialogueSystem.Data.Nodes;
using TMPro;
using UnityEngine;
using DreamSeeker.Framework.Events;
using PlayArk.DialogueSystem.Events;
namespace PlayArk.DialogueSystem.Runtime
{
    [DisallowMultipleComponent]
    public class DialogueBoxPanel : PanelBase_Mini
    {
        [SerializeField] private TextMeshProUGUI m_speakerNameText;
        [SerializeField] private DialogueText m_contentText;

        [Header("CURSOR")]
        [SerializeField] private bool m_isShowCursor = true;
        [SerializeField] private Widget m_nextCursorWidget;
        [SerializeField] private Animator m_nextCursorAnimator;
        [Header("CHOICE")]
        [Tooltip("选项光标")]
        [SerializeField] private RectTransform m_choiceCursorRect;
        [SerializeField] private ChoicesSection m_choiceSection;


        public bool IsPrintShowed => m_contentText.IsPrintShowed;

        #region Main Methods
        public void PrintContent(DialogueNodeNormal.DialogueData data)
        {
            if(m_isShowCursor)
            {
                //点击动画
                m_nextCursorAnimator.SetTrigger(DialogueManager.ClickHash);
                //隐藏下方光标
                m_nextCursorWidget.Fade(0f, 0.5f);
            }
            

            //处理上一句的残留文本
            if (m_contentText.text != "")
            {
                m_contentText.Disappear(() =>
                {
                    //更新说话人名称
                    m_speakerNameText.text = data.SpeakerName;
                    m_contentText.PrintText(data.Content, data.DisplayType);
                });
            }
            else
            {
                //更新说话人名称
                m_speakerNameText.text = data.SpeakerName;
                m_contentText.PrintText(data.Content, data.DisplayType);
            }
        }
        #endregion

        #region Other Methods
        public void QuickShowRemaining()
        {
            m_contentText.QuickShowRemaining();
        }
        /// <summary>
        /// 设置选项光标的位置
        /// </summary>
        /// <param name="position"></param>
        public void SetChoiceCursorRect(Vector3 position)
        {
            m_choiceCursorRect.position = position + new Vector3(-1, 0, 0);
        }
        /// <summary>
        /// 显示对话框部分
        /// </summary>
        public void ShowChoicesSection(ChoiceData[] datas, int defaultSelectIndex)
        {
            //显示选项光标
            m_choiceCursorRect.gameObject.SetActive(true);

            m_choiceSection.Init(datas);
            m_choiceSection.Show(defaultSelectIndex);
        }
        public void HideChoicesSection()
        {
            //隐藏选项光标
            m_choiceCursorRect.gameObject.SetActive(false);

            m_choiceSection.Hide();
        }
        public void ClearContent()
        {
            m_speakerNameText.text = "";
            m_contentText.text = "";
        }
        #endregion


        #region Event
        private void OnEnable()
        {
            EventBus.Subscribe<DialoguePrintCompletedEvent>(OnTextShowed);
        }
        private void OnDisable()
        {
            EventBus.Unsubscribe<DialoguePrintCompletedEvent>(OnTextShowed);
        }

        /// <summary>
        /// 文本打印完成后显示下一步提示光标。
        /// </summary>
        private void OnTextShowed(DialoguePrintCompletedEvent eventData)
        {
            if (m_isShowCursor)
            {
                m_nextCursorWidget.Fade(1f, 1f);
            }
            
        }
        #endregion


        public override void OnHideFadedComplete()
        {

        }
        public override void OnShowFadePreComplete()
        {
            ClearContent();
            if (m_isShowCursor)
            {
                m_nextCursorWidget.Fade(0f, 0f);
            }
            m_choiceCursorRect.gameObject.SetActive(false);
        }
    }
}
