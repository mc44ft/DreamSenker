using System.Collections.Generic;
using PlayArk.GraphCore;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlayArk.StateMachine.Editor
{
    public class StateView : GraphCoreNodeView<StateTransitionEdgeView>
    {
        private VisualElement _header;
        // private VisualElement _runningStateStyle;
        public override void Init(GraphCoreNode data, GraphCoreGraph graphCore)
        {
            //加载状态机节点的USS样式文件
            StyleSheet styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/PlayArk/StateMachine/Editor/StateView.uss");
            styleSheets.Add(styleSheet);
        
            base.Init(data, graphCore);
        
            //在Init结束（端口绘制完之后再查找端口）
            //将端口添加到header下，直接改变其父子关系
            _header = this.Q<VisualElement>("node-header");
            List<Port> ports = this.Query<Port>().ToList();
            foreach (Port port in ports)
            {
                if (CoreNode is EntryState && port.direction == Direction.Input ||
                    CoreNode is AnyState && port.direction == Direction.Input)
                {
                    //禁用该端口
                    port.SetEnabled(false);
                }
                _header.Add(port);
            }
            
            // _runningStateStyle = this.Q<VisualElement>()
        }

        public void UpdateStateInRunning()
        {
            if (Application.isPlaying)
            {
                if (CoreNode is ActionState state && state.Started)
                {
                    _header.AddToClassList("runningState");
                }
                else
                {
                    _header.RemoveFromClassList("runningState");
                }
            }
        }
        protected override void SetStyle()
        {
            base.SetStyle();
            this.AddToClassList("sm-node");
        }

        protected override Port.Capacity GetPortCapacity()
        {
            return CoreNode is EntryState ? Port.Capacity.Single : Port.Capacity.Multi;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            if (Application.isPlaying)
                return;
        
            //只有当右键在节点上时才显示添加连线的选项
            if (evt.target is StateView)
            {
                evt.menu.AppendAction("Add TransitionEdge", a => 
                {
                    DragTransitionEdge();
                });
            }
        
        }
        /// <summary>
        /// Initiates the process of creating a transition edge by sending a drag event to the output port.
        /// 这里通过在上下文菜单中点击菜单项 来模拟 从端口拖拽连线的功能
        /// </summary>
        private void DragTransitionEdge()
        {
            //outputPort.GetGlobalCenter()获取输出端口的全局位置
            //再将outputPort端口作为按下的目标传入给这个集成了鼠标按下事件的类
            //在UIToolkit中 这些事件都是一个个的数据包 通过类来传递
            Port outputPort = (outputContainer[0] as GraphCorePortTemplate)?.ComponentPort;
            outputPort?.SendEvent(
                new DragEvent(outputPort.GetGlobalCenter(), outputPort));
        }

        private class DragEvent : MouseDownEvent
        {
            public DragEvent(Vector2 mousePosition, VisualElement dragTarget)
            {
                this.mousePosition = mousePosition;
                this.target = dragTarget;
            }
        }
    }
}