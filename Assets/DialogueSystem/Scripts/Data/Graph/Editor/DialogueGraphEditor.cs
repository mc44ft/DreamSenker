
using DialogueSystem.Data;
using System;
using XNodeEditor;
namespace DialogueSystem.Editor
{
    [CustomNodeGraphEditor(typeof(DialogueGraph))]
    public class DialogueGraphEditor : NodeGraphEditor
    {
        /// <summary>
        /// 控制哪些节点可以显示在右键菜单中
        /// 将所有DialogueSystem的节点都放在了同一个命名空间下
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override string GetNodeMenuName(Type type)
        {
            if (type.Namespace == "DialogueSystem.Data")
            {
                //正常返回菜单名
                return base.GetNodeMenuName(type);
            }
            else
            {
                return null;
            }
        }
    }

}
