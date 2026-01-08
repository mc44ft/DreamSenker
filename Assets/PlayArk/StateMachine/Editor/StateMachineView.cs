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
            return TypeCache.GetTypesDerivedFrom<State>();
        }

    }
}