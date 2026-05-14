using PlayArk.DialogueSystem.Data.Nodes;
using UnityEngine;

namespace DialogueSystem.Data.Nodes
{
    /// <summary>
    /// 用于通知外部打开UI的节点
    /// </summary>
    public abstract class DialogueNodeExternalUI : DialogueNodeBase
    {
        /// <summary>
        /// 外部 UI 选择结果索引。
        /// </summary>
        private int _selectedIndex;

        protected override void OnExecute()
        {
            //隐藏对话框
            DialogueManager.Instance.FadeOutDialogueBox(0.5f, () =>
            {
                ShowPanel(OnExternalUIFinished);
            });
        }

        /// <summary>
        /// 打开外部 UI，并在 UI 关闭后回调结果索引。
        /// </summary>
        protected abstract void ShowPanel(System.Action<int> onFinished);

        /// <summary>
        /// 外部 UI 操作完成后恢复对话框并结束当前节点。
        /// </summary>
        private void OnExternalUIFinished(int selectedIndex)
        {
            _selectedIndex = selectedIndex;
            
            //显示对话框
            DialogueManager.Instance.FadeInDialogueBox(0.5f, OnFinished);
        }
        

        protected override void Finished()
        {

        }

        /// <summary>
        /// 根据外部 UI 返回索引选择下一个输出端口。
        /// </summary>
        public override string GetNextPortID()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _outputPorts.Count)
            {
                Debug.LogWarning("外部 UI 返回索引越界！");
                return null;
            }

            return _outputPorts[_selectedIndex].GetUniqueID();
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
