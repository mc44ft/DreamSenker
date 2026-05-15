using DialogueSystem.Data.Nodes;
using DialogueSystem;
using PlayArk.GraphCore.Utilities;
using UnityEngine;

using DreamSeeker.QuestSystem.Data;

namespace PlayArk.DialogueSystem.Data.Nodes
{
    [NodeMenuItem("DialogueNodeTaskPublish")]
    public class DialogueNodeTaskPublish : DialogueNodeExternalUI
    {
        /// <summary>
        /// 当前节点要发布的任务配置。
        /// </summary>
        [SerializeField] private QuestDefinitionSO _questDefinition;

        /// <summary>
        /// 打开任务发布面板。
        /// </summary>
        protected override void ShowPanel(System.Action<int> onFinished)
        {
            DialogueManager.Instance.ExternalUIRunner.OpenTaskPanel(
                _questDefinition,
                DreamSeeker.UI.Panels.TaskPanel.E_PanelMode.Publish,
                onFinished);
        }
    }
}
