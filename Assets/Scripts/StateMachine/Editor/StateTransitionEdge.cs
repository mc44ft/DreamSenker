using System.Collections;
using System.Collections.Generic;
using PlayArk.GraphCore.Data;
using PlayArk.GraphCore.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class StateTransitionEdge : GraphCoreEdgeView
{
    private const int ARROW_WIDTH = 12;

    public StateTransitionEdge()
    {
        //几何结构改变事件
        //当连线的位置或尺寸发生改变时 触发
        edgeControl.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        //这里是为了消除连线的弧度
        //在GraphView中 一条连线通常是由四个点构成的 贝塞尔曲线
        //[0]是起始点（起点端口的位置）
        //[1]是起始点的控制点（控制线从起始点出来时的弧度）
        //[2]是终点的控制点（控制线进入重点的弧度）
        //[3]是终点
        //这里将控制点改为了起始点的和终点位置，消除了默认弧度
        PointsAndTangents[1] = PointsAndTangents[0];
        PointsAndTangents[2] = PointsAndTangents[3];
        
        
    }
}
