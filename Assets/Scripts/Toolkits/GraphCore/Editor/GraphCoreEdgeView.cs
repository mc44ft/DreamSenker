
using UnityEditor.Experimental.GraphView;

public class GraphCoreEdgeView : Edge
{
    public GraphCoreEdge GraphCoreEdge { get; private set; }
    public void BindData(GraphCoreEdge graphCoreEdge)
    {
        GraphCoreEdge = graphCoreEdge;
    }
}
