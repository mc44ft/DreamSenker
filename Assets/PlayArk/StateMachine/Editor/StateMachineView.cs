using System.Collections.Generic;
using PlayArk.GraphCore;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;

namespace PlayArk.StateMachine.Editor
{
    public class StateMachineView : GraphCoreView<StateView, StateTransitionEdgeView>
    {
        protected override bool InterceptionGraph(GraphCoreGraph graphCore)
        {
            if (graphCore is global::PlayArk.StateMachine.StateMachine)
                return false;
            return true;
        }

        protected override TypeCache.TypeCollection GetMenuNodeType()
        {
            //得到所有State的派生类型的反射信息
            //获取所有GraphCoreNode的子类及其本身
            return TypeCache.GetTypesDerivedFrom<State>();
        }

        public void UpdateStates()
        {
            foreach (var node in nodes)
            {
                if (node is StateView stateView)
                {
                    stateView.UpdateStateInRunning();
                }
            }
        }
    }
}