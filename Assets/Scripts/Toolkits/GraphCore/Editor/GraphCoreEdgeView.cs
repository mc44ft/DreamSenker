using PlayArk.GraphCore.Data;
using UnityEditor.Experimental.GraphView;

namespace PlayArk.GraphCore.Editor
{
    public class GraphCoreEdgeView : Edge
    {
        public GraphCoreEdge GraphCoreEdge { get; private set; }
        public void BindData(GraphCoreEdge graphCoreEdge)
        {
            GraphCoreEdge = graphCoreEdge;
        }
    }
}