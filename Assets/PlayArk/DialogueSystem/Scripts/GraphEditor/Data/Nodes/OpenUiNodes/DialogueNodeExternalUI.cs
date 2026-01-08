using PlayArk.DialogueSystem.Data.Nodes;
using UnityEngine;
namespace DialogueSystem.Data.Nodes
{
    /// <summary>
    /// 用于通知外部打开UI的节点
    /// </summary>
    public abstract class DialogueNodeExternalUI : DialogueNodeBase
    {
        // [Tooltip("想要显示的UI面板类型")]
        // [SerializeField] private E_DialogueExternalUiPanelType m_panelType;

        private int _nextNodeIndex = 0;
        protected override void OnExecute()
        {
            //监听外部面板操作完成的事件
            EventCenter.Instance.AddEventListener<DialoguePanelFinishedEventArgs>(E_EventType.Dialogue_PanelFinished, OnPanelFinished);
            
            //隐藏对话框
            DialogueManager.Instance.FadeOutDialogueBox(0.5f, () =>
            {
                ShowPanel();
            });
            
        }

        protected abstract void ShowPanel();
        private void OnPanelFinished(object eventSender, DialoguePanelFinishedEventArgs args)
        {
            _nextNodeIndex = args.Index;
            
            //显示对话框
            DialogueManager.Instance.FadeInDialogueBox(0.5f);
            //外部面板操作完成后结束该节点
            OnFinished();
        }
        

        protected override void Finished()
        {
            //取消监听外部面板操作完成的事件
            EventCenter.Instance.RemoveEventListener<DialoguePanelFinishedEventArgs>(E_EventType.Dialogue_PanelFinished, OnPanelFinished);
        }
        // /// <summary>
        // /// 这里简单重写了获取下一个节点的方法，用于应对策划需求 后续还需要根据自己的UIToolkit重做
        // /// </summary>
        // /// <returns></returns>
        // public override string GetNextPortID()
        // {
        //     if(m_panelType == E_DialogueExternalUiPanelType.TaskPublishPanel)
        //     {
        //         NodePort outputPort = GetOutputPort("Output");
        //         if (outputPort == null || !outputPort.IsConnected)
        //             return null;
        //         if (_nextNodeIndex < 0 || _nextNodeIndex >= outputPort.ConnectionCount)
        //             return null;
        //         NodePort connection = outputPort.GetConnection(_nextNodeIndex);
        //         return connection.node as DialogueNodeBase;
        //     }
        //     else
        //     {
        //         return base.GetNextPortID();
        //     }
        // }
    }
}

