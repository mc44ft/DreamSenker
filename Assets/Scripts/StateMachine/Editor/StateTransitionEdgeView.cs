
using PlayArk.GraphCore.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class StateTransitionEdgeView : GraphCoreEdgeView
{
    private const float ARROW_WIDTH = 12;//三角形箭头的边长
    private const float EDGE_OFFSET = 4;//连线的偏移量
    private const float SELF_ARROW_OFFSET = 35;//自循环箭头的偏移量
    public StateTransitionEdgeView()
    {
        //几何结构改变事件
        //当连线的位置或尺寸发生改变时 触发
        edgeControl.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        //视觉内容生成回调
        //当该元素需要重新绘制时 调用
        //重新绘制箭头
        generateVisualContent += DrawArrow;
    }

    public override void OnSelected()
    {
        base.OnSelected();
        
        //当连线被选中时 创建临时的代理对象
        var helper = ScriptableObject.CreateInstance<EdgeInspectorHelper>();
        helper.name = "Transition Edge";
        helper.Data = GraphCoreEdge as StateTransitionEdge;
        helper.StateMachine = _graphCore as StateMachine;

        //偷梁换柱
        Selection.activeObject = helper;
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        //这里是为了消除连线的弧度
        //在GraphView中 一条连线通常是由四个点构成的 贝塞尔曲线
        //[0]是起始点（起点端口的位置）
        //[1]是起始点的控制点（控制线从起始点出来时的弧度）
        //[2]是终点的控制点（控制线进入终点的弧度）
        //[3]是终点
        //这里将控制点改为了起始点和终点位置，消除了默认弧度
        //[0][3]这两个点是死的
        //[1][2]这两个点是活的
        //后面的连线位置偏移就是靠改变这两个点的位置决定的
        PointsAndTangents[1] = PointsAndTangents[0];
        PointsAndTangents[2] = PointsAndTangents[3];
        if (input != null && output != null)
        {
            AddHorizontalOffset();
            AddVerticalOffset();
        }
        
        //强制重新绘制连线
        //这样才能在用户移动连线的时候同步移动箭头
        MarkDirtyRepaint();
    }
    private void AddHorizontalOffset()
    {
        if (input.node.GetPosition().x > output.node.GetPosition().x)
        {
            PointsAndTangents[1].y -= EDGE_OFFSET;
            PointsAndTangents[2].y -= EDGE_OFFSET;
        }
        else if (input.node.GetPosition().x < output.node.GetPosition().x)
        {
            PointsAndTangents[1].y += EDGE_OFFSET;
            PointsAndTangents[2].y += EDGE_OFFSET;
        }
    }
    private void AddVerticalOffset()
    {
        if (input.node.GetPosition().y > output.node.GetPosition().y)
        {
            PointsAndTangents[1].x += EDGE_OFFSET;
            PointsAndTangents[2].x += EDGE_OFFSET;
        }
        else if (input.node.GetPosition().y < output.node.GetPosition().y)
        {
            PointsAndTangents[1].x -= EDGE_OFFSET;
            PointsAndTangents[2].x -= EDGE_OFFSET;
        }
    }
    private void DrawArrow(MeshGenerationContext context)
    {
        //计算连线的中点位置和方向
        //这里使用了[1]和[2]这两个切线点，因为前面已经将这两个切线点做了偏移 连线的线段对应的就是这两点之间
        Vector2 start = PointsAndTangents[PointsAndTangents.Length / 2 - 1];
        Vector2 end = PointsAndTangents[PointsAndTangents.Length / 2];
        Vector2 mid = (start + end) / 2;//连线中点 三角形的底边中心
        Vector2 direction = end - start;

        if (IsSelfTransition())//这里要画自循环箭头
        {
            //在UIToolkit的坐标系里 左上角是原点 y轴的正方向是向下的
            mid = PointsAndTangents[0] + Vector2.up * SELF_ARROW_OFFSET;
            direction = Vector2.down;
        }
        
        //箭头是一个等边三角形
        //等边三角形的高度是 二分之根号三 * 边长
        //这里计算出了箭头的一半高度
        //将箭头的几何中心居中
        float distanceFromMid = ARROW_WIDTH * Mathf.Sqrt(3) / 4;
        
        //计算和连线垂直的向量
        Vector2 perpendicular = new Vector2(-direction.y, direction.x).normalized;

        if (IsSelfTransition())
        {
            perpendicular = Vector2.right;
        }
        //申请三个网格顶点和三个索引
        MeshWriteData mesh = context.Allocate(3, 3);
        Vertex[] vertices = new Vertex[3];
        
        //计算出箭头三角形的三个顶点位置
        vertices[0].position = mid + (direction.normalized * distanceFromMid);
        vertices[1].position = mid + (-direction.normalized * distanceFromMid) +
                               (perpendicular.normalized * ARROW_WIDTH / 2);
        vertices[2].position = mid + (-direction.normalized * distanceFromMid) +
                               (-perpendicular.normalized * ARROW_WIDTH / 2);
        for (int i = 0; i < vertices.Length; i++)
        {
            //解决 深度冲突/闪烁问题
            //显卡在渲染时 由于两个物体的深度完全一致 它会纠结到底该显示连线的像素还是箭头的像素
            //这会导致画面旋转或缩放时，箭头出现闪烁、被遮挡或边缘破碎的现象
            //Vertex.nearZ不是一个向量，而是一个极其微小的float值
            //是Unity专门为了解决这个问题而进行适配的数值
            //它可以保证在不触发摄像机裁剪的前提下，实现最稳定的遮挡关系
            vertices[i].position += Vector3.forward * Vertex.nearZ;
            vertices[i].tint = GetColor();
        }
        //录入顶点信息
        mesh.SetAllVertices(vertices);
        //确定连线顺序
        //以012的顺序连接这几个点
        mesh.SetAllIndices(new ushort[] {0, 1, 2});
    }
    /// <summary>
    /// 判断是否是自己和自己连接
    /// </summary>
    /// <returns></returns>
    private bool IsSelfTransition()
    {
        if (input != null && output != null)
        {
            return input.node == output.node;
        }

        return false;
    }
    /// <summary>
    /// 保持箭头和连线的颜色一致
    /// </summary>
    /// <returns></returns>
    private Color GetColor()
    {
        //这里是一个后来者居上的优先级逻辑
        //越往后优先级越高
        
        Color color = defaultColor;//这是连线的默认颜色
        if(output != null)
        {
            color = output.portColor;//端口颜色
        }

        if (selected)
        {
            color = selectedColor;//选中时的颜色
        }

        if (isGhostEdge)
        {
            color = ghostColor;
        }

        return color;
    }
}
