using System;
using UnityEngine;

using DreamSenker.QuestSystem.Data;
using DreamSenker.UI.Panels;
using PlayArk.DialogueSystem.Runtime;

namespace DreamSenker.Dialogue
{
/// <summary>
/// 对话外部 UI 协调器，负责打开面板并把面板结果回传给对话节点。
/// </summary>
public class DialogueExternalUIRunner
{
    /// <summary>
    /// 打开普通外部 UI 面板。
    /// </summary>
    public void OpenExternalPanel(E_DialogueExternalUiPanelType panelType, Action<int> onFinished)
    {
        switch (panelType)
        {
            case E_DialogueExternalUiPanelType.BounsChoosePanel:
                UIManager.Instance.ShowPanel<BounsChoosePanel>(E_UILayer.Top, panel =>
                {
                    panel.Initialize(onFinished);
                });
                break;
            case E_DialogueExternalUiPanelType.RestoreHealthPanel:
                UIManager.Instance.ShowPanel<RestoreHealthPanel>(E_UILayer.Top, panel =>
                {
                    panel.Initialize(onFinished);
                });
                break;
            default:
                Debug.LogError($"DialogueExternalUIRunner 不支持的普通外部面板：{panelType}");
                onFinished?.Invoke(0);
                break;
        }
    }

    /// <summary>
    /// 打开任务面板。
    /// </summary>
    public void OpenTaskPanel(QuestDefinitionSO questDefinition, TaskPanel.E_PanelMode mode, Action<int> onFinished)
    {
        if (questDefinition == null)
        {
            Debug.LogError("打开任务面板失败：QuestDefinitionSO 为空");
            onFinished?.Invoke(0);
            return;
        }

        UIManager.Instance.ShowPanel<TaskPanel>(E_UILayer.Top, panel =>
        {
            panel.Initialize(questDefinition, mode, onFinished);
        });
    }
}
}
