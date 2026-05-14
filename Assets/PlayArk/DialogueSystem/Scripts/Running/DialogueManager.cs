using System;
using PlayArk.DialogueSystem.Data;
using PlayArk.DialogueSystem.Data.Nodes;
using PlayArk.DialogueSystem.Runtime;
using UnityEngine;
using UnityEngine.UI;

using DreamSenker.Dialogue;

namespace DialogueSystem
{
    public class DialogueManager : SingletonMono<DialogueManager>
    {
        private DialogueGraph _currentRunningDialogueGraph;

        private DialogueBoxPanel _dialogueBoxPanel;

        /// <summary>
        /// 外部 UI 协调器，用于对话节点临时转交控制权。
        /// </summary>
        public DialogueExternalUIRunner ExternalUIRunner { get; private set; }

        /// <summary>
        /// 当前聚焦的Button
        /// </summary>
        private Selectable _currentSelectButton;
        public static int ClickHash = Animator.StringToHash("Click");

        protected override void Awake()
        {
            base.Awake();
            ExternalUIRunner = new DialogueExternalUIRunner();
        }

        /// <summary>
        /// 控制玩家当前能否交互
        /// </summary>
        private bool _canInteractable;
        /// <summary>
        /// 玩家当前能否快速显示下一句话
        /// </summary>
        private bool _canQuickShow;

        /// <summary>
        /// 一切的开始 开始执行对话图
        /// </summary>
        /// <param name="onFinished">bool参数 是否保存对话记录</param>
        public void PlayDialogueGraph(DialogueGraph dialogueGraph, Action<bool> onFinished = null)
        {
            _currentRunningDialogueGraph = dialogueGraph;
            ShowDialogueBox(() =>
            {
                _currentRunningDialogueGraph.Initialize((isSuccess, shouldSave) =>
                {
                    if (isSuccess)
                    {
                        HideDialogueBox();
                        onFinished?.Invoke(shouldSave);
                    }
                    else
                    {
                        Debug.LogWarning("对话图执行失败");
                        HideDialogueBox();
                        onFinished?.Invoke(false);
                    }
                });
                _currentRunningDialogueGraph.Execute();
            });
        }
        private void ShowDialogueBox(Action callback)
        {
            UIManager.Instance.ShowPanel<DialogueBoxPanel>(E_UILayer.Top, (panel) =>
            {
                _dialogueBoxPanel = panel;
            }, (panel) =>
            {
                callback();
            });
        }
        private void HideDialogueBox()
        {
            _canInteractable = false;
            

            UIManager.Instance.HidePanel<DialogueBoxPanel>((panel) =>
            {
                //清空对话框
                panel.ClearContent();
                _dialogueBoxPanel = null;
            }, null);
        }
        /// <summary>
        /// 通过透明度来隐藏对话框（淡出对话框）
        /// </summary>
        public void FadeOutDialogueBox(float duration = 0.5f, Action onFinished = null)
        {
            if(_dialogueBoxPanel.TryGetComponent(out Widget widget))
            {
                widget.Fade(0f, duration, onFinished);
            }
        }
        /// <summary>
        /// 通过透明度来显示对话框（淡入对话框）
        /// </summary>
        public void FadeInDialogueBox(float duration = 0.5f, Action onFinished = null)
        {
            if(_dialogueBoxPanel.TryGetComponent(out Widget widget))
            {
                widget.Fade(1f, duration, onFinished);
            }
        }
        public void ShowDialogueChoicesSection(ChoiceData[] datas, int defaultSelectIndex)
        {
            _dialogueBoxPanel.ShowChoicesSection(datas, defaultSelectIndex);
        }
        public void HideDialogueChoicesSection()
        {
            _dialogueBoxPanel.HideChoicesSection();
        }
        private void Update()
        {
            //此处为测试用
            //if (Input.GetKeyDown(KeyCode.P) && _dialogueBoxPanel == null)
            //{
            //    PlayDialogueGraph();
            //}

            if (_canInteractable)
                UpdateInput();
        }
        private void UpdateInput()
        {
            if (Input.GetButtonDown("Submit"))//默认是手柄A键 键盘Return键 Space键
            {
                #region 说明
                //玩家按下Submit键
                //（Typing）
                //此时对话未打印完 不执行任何操作
                //此时对话打印完了 显示下一句话
                //（Other）
                //此时对话未打印完 不执行任何操作
                //此时对话打印完了 显示下一句话
                #endregion

                if (_dialogueBoxPanel.IsPrintShowed)//当前打印完了才能继续打印下一句话
                {
                    //打印下一句话
                    EventCenter.Instance.EventTrigger(E_EventType.Dialogue_ContentNext, this, new EmptyEventArgs());
                }
            }
            if (Input.GetButtonDown("Cancel"))
            {
                #region 说明
                //玩家按下Cancel键
                //（Typing - 当前语句支持快速显示）
                //此时对话未打印完 快速显示剩余未打印部分
                //此时对话打印完了 如果下一句话支持快速显示 则直接显示下一句话
                //                如果下一句话不支持快速显示 则按照正常显示
                //（Typing - 当前语句不支持快速显示）
                //此时对话未打印完 不执行任何操作
                //此时对话打印完了 如果下一句话支持快速显示 则直接显示下一句话
                //                如果下一句话不支持快速显示 则按照正常显示
                //（Other - 快速显示此时无意义）
                //此时对话未打印完 不执行任何操作
                //此时对话打印完了 如果下一句话支持快速显示 则直接显示下一句话
                //                如果下一句话不支持快速显示 则按照正常显示
                #endregion
                //Debug.Log("快速显示" + _canQuickShow + " " + _dialogueBoxPanel.IsPrintShowed);
                if (_canQuickShow && !_dialogueBoxPanel.IsPrintShowed)//支持快速打印 并且 当前没有打印完毕
                {
                    
                    _dialogueBoxPanel.QuickShowRemaining();
                }
                else if(_dialogueBoxPanel.IsPrintShowed)//如果当前打印完了 Cancel键的作用和Submit的作用一样
                {
                    EventCenter.Instance.EventTrigger(E_EventType.Dialogue_ContentNext, this, new EmptyEventArgs());
                }
            }
        }
        /// <summary>
        /// 打印下一句话
        /// </summary>
        /// <param name="data"></param>
        public void PrintNextContent(DialogueNodeNormal.DialogueData data)
        {
            //更新输入配置
            _canQuickShow = data.CanQuickShow;
            if(data.DisplayType == DialogueText.E_DisplayType.Typing)
                _canInteractable = true;//如果是逐字打印 则开放玩家输入
            else
                _canInteractable = false;//否则禁止玩家输入

            _dialogueBoxPanel.PrintContent(data);
                
        }
        public void SetCurrentSelectButton(Selectable selectable)
        {
            _currentSelectButton = selectable;
            if (selectable != null)
                _dialogueBoxPanel.SetChoiceCursorRect(selectable.transform.position);
        }

        private void OnEnable()
        {
            EventCenter.Instance.AddEventListener<EmptyEventArgs>(E_EventType.Dialogue_PrintShowed, OnTextShowed);
        }
        private void OnDisable()
        {
            EventCenter.Instance.RemoveEventListener<EmptyEventArgs>(E_EventType.Dialogue_PrintShowed, OnTextShowed);
        }

        private void OnTextShowed(object eventSender, EmptyEventArgs args)
        {
            _canInteractable = true;
        }
    }
}
