using System.Collections.Generic;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor;
public class StateMachineView : GraphCoreView<StateView, StateTransitionEdgeView>
{
    protected override bool InterceptionGraph(GraphCoreGraph graphCore)
    {
        if (graphCore is StateMachine)
            return false;
        return true;
    }

    protected override TypeCache.TypeCollection GetMenuNodeType()
    {
        return TypeCache.GetTypesDerivedFrom<State>();
    }

}
