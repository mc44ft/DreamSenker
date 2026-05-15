using DialogueSystem.Data.Nodes;
using DialogueSystem;
using PlayArk.GraphCore.Utilities;
using UnityEngine;

using DreamSeeker.QuestSystem.Data;

namespace PlayArk.DialogueSystem.Data.Nodes
{
[NodeMenuItem("DialogueNodeTaskDeliver")]
public class DialogueNodeTaskDeliver : DialogueNodeExternalUI
{
    /// <summary>
    /// 当前节点要交付的任务配置。
    /// </summary>
    [SerializeField] private QuestDefinitionSO _questDefinition;

    /// <summary>
    /// 打开任务交付面板。
    /// </summary>
    protected override void ShowPanel(System.Action<int> onFinished)
    {
        DialogueManager.Instance.ExternalUIRunner.OpenTaskPanel(
            _questDefinition,
            DreamSeeker.UI.Panels.TaskPanel.E_PanelMode.Deliver,
            onFinished);
    }
}
}
