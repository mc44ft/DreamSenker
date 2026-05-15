using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using DreamSeeker.Managers;
using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.UI.Panels
{
public class TaskPanel : PanelBase_Mini
{
    [Header("CONTAINER")]
    [SerializeField] private GameObject _publishButtons;//发布任务面板的按钮集合
    [SerializeField] private GameObject _deliverButtons;//提交任务面板的按钮集合
    
    [Header("ELEMENT")]
    [SerializeField] private Button _acceptButton;
    [SerializeField] private Button _rejectButton;
    [SerializeField] private Button _deliverButton;

    [Header("TEXT")]
    [SerializeField] private TextMeshProUGUI _titleText;//任务标题文本，可为空
    [SerializeField] private TextMeshProUGUI _descriptionText;//任务描述文本，可为空
    [SerializeField] private TextMeshProUGUI _targetDescriptionText;//任务目标文本，可为空

    private QuestDefinitionSO _questDefinition;//当前面板展示的任务配置
    private Action<int> _onFinished;//面板关闭后返回给对话节点的结果回调

    /// <summary>
    /// 用任务配置和面板模式初始化任务面板。
    /// </summary>
    public void Initialize(QuestDefinitionSO questDefinition, E_PanelMode mode, Action<int> onFinished)
    {
        _questDefinition = questDefinition;
        _onFinished = onFinished;

        RefreshText();
        ClearButtonListeners();
        _publishButtons.SetActive(false);
        _deliverButtons.SetActive(false);

        switch (mode)
        {
            case E_PanelMode.Publish:
                _publishButtons.SetActive(true);
                _acceptButton.onClick.RemoveListener(AcceptOnClick);
                _rejectButton.onClick.RemoveListener(RejectOnClick);
                _acceptButton.onClick.AddListener(AcceptOnClick);
                _rejectButton.onClick.AddListener(RejectOnClick);
                break;
            case E_PanelMode.Deliver:
                _deliverButtons.SetActive(true);
                _deliverButton.onClick.RemoveListener(DeliverOnClick);
                _deliverButton.onClick.AddListener(DeliverOnClick);
                break;
        }
    }

    /// <summary>
    /// 清理按钮旧监听，避免面板复用时重复触发。
    /// </summary>
    private void ClearButtonListeners()
    {
        _acceptButton.onClick.RemoveListener(AcceptOnClick);
        _rejectButton.onClick.RemoveListener(RejectOnClick);
        _deliverButton.onClick.RemoveListener(DeliverOnClick);
    }

    /// <summary>
    /// 根据任务配置刷新面板文本。
    /// </summary>
    private void RefreshText()
    {
        TextMeshProUGUI fallbackText = GetFallbackText();
        bool hasExplicitTextField = _titleText != null || _descriptionText != null || _targetDescriptionText != null;

        if (!hasExplicitTextField && fallbackText != null)
        {
            fallbackText.text = BuildFallbackContent();
            return;
        }

        if (_titleText != null)
        {
            _titleText.text = _questDefinition != null ? _questDefinition.Title : string.Empty;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = _questDefinition != null ? _questDefinition.Description : string.Empty;
        }

        if (_targetDescriptionText != null)
        {
            _targetDescriptionText.text = _questDefinition != null ? _questDefinition.TargetDescription : string.Empty;
        }
    }

    /// <summary>
    /// 获取未配置文本引用时使用的兜底文本组件。
    /// </summary>
    private TextMeshProUGUI GetFallbackText()
    {
        return GetComponentInChildren<TextMeshProUGUI>(true);
    }

    /// <summary>
    /// 构建单文本组件下的任务展示内容。
    /// </summary>
    private string BuildFallbackContent()
    {
        if (_questDefinition == null)
        {
            return string.Empty;
        }

        return $"{_questDefinition.Title}\n{_questDefinition.Description}\n{_questDefinition.TargetDescription}".Trim();
    }

    /// <summary>
    /// 获取当前任务 ID，配置缺失时输出错误。
    /// </summary>
    private bool TryGetQuestId(out string questId)
    {
        questId = _questDefinition != null ? _questDefinition.QuestId : string.Empty;
        if (!string.IsNullOrWhiteSpace(questId))
        {
            return true;
        }

        Debug.LogError("任务面板操作失败：QuestDefinitionSO 或 QuestId 为空");
        return false;
    }

    private void AcceptOnClick()
    {
        if (!TryGetQuestId(out string questId) || !QuestManager.Instance.StartQuest(questId))
        {
            return;
        }

        //接受任务
        UIManager.Instance.GetPanel<GamePanel>((panel) =>
        {
            panel.SetTopBarActive(true, _questDefinition.TargetDescription);
        });

        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        ClosePanel(0);
    }
    private void RejectOnClick()
    {
        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        ClosePanel(1);
    }
    private void DeliverOnClick()
    {
        if (!TryGetQuestId(out string questId) || !QuestManager.Instance.CompleteQuest(questId))
        {
            return;
        }

        //交付任务
        UIManager.Instance.GetPanel<GamePanel>((panel) =>
        {
            panel.SetTopBarActive(false);
        });

        AudioManager.Instance.PlaySound(GameResources.Instance.UiButtonClip);

        ClosePanel(0);
    }

    /// <summary>
    /// 关闭面板，并在淡出完成后把结果返回给对话节点。
    /// </summary>
    private void ClosePanel(int resultIndex)
    {
        Action<int> finishedCallback = _onFinished;
        _onFinished = null;

        UIManager.Instance.HidePanel<TaskPanel>(null, () =>
        {
            finishedCallback?.Invoke(resultIndex);
        });
    }
    public override void OnHideFadedComplete()
    {
        
    }

    public override void OnShowFadePreComplete()
    {
        
    }


    public enum E_PanelMode
    {
        /// <summary>
        /// 发布任务状态
        /// 接受/拒绝
        /// </summary>
        Publish,
        /// <summary>
        /// 交付任务状态
        /// 交付
        /// </summary>
        Deliver,
    }
}
}
