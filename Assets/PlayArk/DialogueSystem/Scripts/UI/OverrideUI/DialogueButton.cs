using DialogueSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DreamSeeker.Framework.Events;
using PlayArk.DialogueSystem.Events;
namespace PlayArk.DialogueSystem.Runtime
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(DialogueButtonCenter))]
    public class DialogueButton : Button
    {
        private Animator _animator;
        private DialogueButtonCenter _center;

        private int _clickHash = Animator.StringToHash("Click");
        /// <summary>
        /// 该选项的索引（从上到下）
        /// </summary>
        private int _index;
        public virtual void Init(string content, int index)
        {
            _index = index;
            _center.ContentText.PrintText(content, DialogueText.E_DisplayType.Default);
        }

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _center = GetComponent<DialogueButtonCenter>();
        }
        protected override void Start()
        {
            base.Start();
            onClick.AddListener(OnClick);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            _center.FrontRingWidget.Fade(1f, 0.1f);
            DialogueManager.Instance.SetCurrentSelectButton(this);
        }
        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            _center.FrontRingWidget.Fade(0f, 0.25f);
        }
        /// <summary>
        /// 鼠标进入
        /// </summary>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            //鼠标进入时 选中选项
            //默认就是一次只能聚焦一个选项的
            Select();
        }
        private void OnClick()
        {
            //播放点击动画
            _animator.SetTrigger(_clickHash);
        }
        /// <summary>
        /// 播放按钮点击动画的对应帧数执行
        /// </summary>
        private void OnConfirm()
        {
            EventBus.Publish(new DialogueChoiceSelectedEvent(_index));
        }
    }

}
