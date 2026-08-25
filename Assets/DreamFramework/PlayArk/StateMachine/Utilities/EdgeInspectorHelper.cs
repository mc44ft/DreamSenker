
using UnityEngine;

namespace PlayArk.StateMachine.Utilities
{
    /// <summary>
    /// 该类用于充当连线Inspector的显示资源
    /// 因为Unity的Selection.activeObject 和 Inspector面板只认 UnityEngine.Object (MonoBehaviour、ScriptableObject等)
    /// 但是连线类只是一个普通的C#类 所以无法作为资源单独显示在Inspector面板上
    /// 这里通过使用一个临时的ScriptableObject作为一个代理来使连线拥有一个临时资源 可以显示在Inspector窗口中
    /// </summary>
    public class EdgeInspectorHelper : ScriptableObject
    {
        //持有真正需要编辑的数据
        public StateTransitionEdge Data;
        //持有主资源引用 用于触发保存和Undo
        [HideInInspector] public global::PlayArk.StateMachine.StateMachine StateMachine;
    }
}