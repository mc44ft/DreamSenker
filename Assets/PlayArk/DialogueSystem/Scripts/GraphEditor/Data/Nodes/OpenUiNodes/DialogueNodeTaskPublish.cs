using System.Collections;
using System.Collections.Generic;
using DialogueSystem.Data;
using DialogueSystem.Data.Nodes;
using PlayArk.DialogueSystem.Runtime;
using UnityEngine;

namespace PlayArk.DialogueSystem.Data.Nodes
{
    public class DialogueNodeTaskPublish : DialogueNodeExternalUI
    {
        protected override void ShowPanel()
        {
            //通知外部打开面板
            EventCenter.Instance.EventTrigger(
                E_EventType.Dialogue_ShowPanel, 
                this, 
                new DialogueShowPanelEventArgs(E_DialogueExternalUiPanelType.TaskPublishPanel));
        }
    }
}