using PlayArk.GraphCore.Data;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace PlayArk.GraphCore.Editor
{
    public class GraphCoreView : GraphView
    {
        //有了这行代码，这个 C# 脚本就变成了一个可以被拖拽的 UI 组件，出现在了 UI Builder 的零件库里
        new class UxmlFactory : UxmlFactory<GraphCoreView, UxmlTraits> { }

        private GraphCoreGraph _graphCore;

        public GraphCoreView()
        {
            //必须有这句代码 才能显示出网格 
            //不是已经在uxml文件里添加了吗？.......
            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(GraphCoreEditor.GetPath() + "GraphCoreEditor.uss");
            styleSheets.Add(styleSheet);

            //添加网格背景
            Insert(0, new GridBackground());
            //添加操纵器
            this.AddManipulator(new ContentZoomer());//鼠标滚轮缩放画布
            this.AddManipulator(new ContentDragger());//按住鼠标中键拖拽画布
            this.AddManipulator(new SelectionDragger());//左键框选并拖拽节点

            //这里订阅了Unity的全局撤销/重做事件
            //用于表现层和数据层同步
            //在用户Ctrl+Z或Ctrl+Y重做时会被触发
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        /// <summary>
        /// 根据GraphCoreSO的数据刷新画布
        /// 切换资源时 或者 更新当前资源时都会调用
        /// 撤销重做时
        /// </summary>
        public void Refresh(GraphCoreGraph graphCore)
        {
            _graphCore = graphCore;

            graphViewChanged -= OnGraphViewChanged;
            //先取消订阅 再执行此函数 不然执行此函数后会自动调用OnGraphViewChanged 导致所有数据全都被误格式化
            //删除画布中所有元素
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            //重新绘制所有元素
            if(_graphCore != null)
            {
                //绘制所有节点
                foreach (var node in _graphCore.GetNodesInternal())
                {
                    DrawNode(node);
                }

                //绘制所有连线
                foreach (var edge in _graphCore.GetEdgesInternal())
                {
                    DrawEdge(edge);
                }
            }
        }
        /// <summary>
        /// 当画布发生改变时调用 添加连线 删除连线或节点时
        /// 此函数被调用时，表现层的东西在Unity的GraphView系统中已经进行了处理
        /// 此函数就是用来通知处理数据层的
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            var edgesToCreate = graphViewChange.edgesToCreate;
            if(edgesToCreate != null)
            {
                foreach (var edge in edgesToCreate)
                {
                    CreateEdge(edge);
                }
            }

            //被删除的元素
            var elementsToRemove = graphViewChange.elementsToRemove;
            if(elementsToRemove != null)
            {
                foreach (var element in elementsToRemove)
                {
                    //节点
                    if (element is GraphCoreNodeView nodeView)
                    {
                        DeleteNode(nodeView);
                    }
                    //连线
                    if(element is GraphCoreEdgeView edge)
                    {
                        DeleteEdge(edge);
                    }
                }
            }

            //延迟刷新视图 防止冲突
            UnityEditor.EditorApplication.delayCall += () =>
            {
                //手动刷新视图 因为会一并删除连线
                Refresh(_graphCore);
            };

            return graphViewChange;
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Application.isPlaying) return;

            base.BuildContextualMenu(evt);

            //解决画布缩放拖拽和屏幕尺寸不同步的问题
            Vector2 mousePosition =
                viewTransform.matrix.//代表了当前画布的状态
                inverse.//逆矩阵
                MultiplyPoint(evt.mousePosition);//矩阵计算
            //添加菜单项
            AppendMenuAction(evt, mousePosition);
            

        }

        protected virtual void AppendMenuAction(ContextualMenuPopulateEvent evt, Vector2 mousePosition) { }

        /// <summary>
        /// 在视图中拉起一根线时 这个方法返回的端口即为允许连接的端口
        /// </summary>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            //ports是整张view上所有Port类型的元素
            foreach (var endPort in ports)
            {
                //排除方向不同的节点
                if (endPort.direction == startPort.direction)
                    continue;
                //排除自身节点
                if ((endPort.userData as GraphCorePort)?.GetSeleNodeID() == (startPort.userData as GraphCorePort)?.GetSeleNodeID())
                    continue;
                //排除已经连接的节点
                if (AreConnected(startPort, endPort))
                    continue;
                compatiblePorts.Add(endPort); 
            }
            return compatiblePorts;
        }
        private bool AreConnected(Port startPort, Port endPort)
        {
            foreach (var edge in _graphCore.GetEdgesInternal())
            {
                if(edge.RootPortID == startPort.viewDataKey && edge.ConnectionPortID == endPort.viewDataKey)
                {
                    return true;
                }
            }
            return false;
        }
        public GraphCoreNodeView GetNodeViewByID(string nodeID)
        {
            return GetNodeByGuid(nodeID) as GraphCoreNodeView;
        }

        
        private void DrawNode(GraphCoreNode node)
        {
            GraphCoreNodeView nodeView = new GraphCoreNodeView(node, _graphCore);
            AddElement(nodeView);
        }
        protected void CreateNode(Type nodeType, Vector2 mousePosition)
        {
            GraphCoreNode node = _graphCore.CreateNodeInternal(nodeType, mousePosition);
            DrawNode(node);
        }
        private void DeleteNode(GraphCoreNodeView nodeView)
        {
            _graphCore.DeleteNodeInternal(nodeView.CoreNode);

            
        }
        private void DrawEdge(GraphCoreEdge edge)
        {
            GraphCoreNodeView rootNodeView = GetNodeViewByID(edge.RootNodeID);
            GraphCoreNodeView trueNodeView = GetNodeViewByID(edge.ConnectionNodeID);
            //建立连接
            GraphCoreEdgeView edgeView = rootNodeView.ConnectTo(edge.RootPortID, trueNodeView, edge.ConnectionPortID);
            edgeView.BindData(edge);
            edgeView.viewDataKey = edge.UniqueID;

            AddElement(edgeView);
        }
        private void CreateEdge(Edge edge)
        {
            GraphCorePort rootPort = edge.output.userData as GraphCorePort;
            GraphCorePort truePort = edge.input.userData as GraphCorePort;
            _graphCore.CreateEdgeInternal(rootPort.GetSeleNodeID(), rootPort.GetUniqueID(), truePort.GetSeleNodeID(), truePort.GetUniqueID());
        }
        private void DeleteEdge(GraphCoreEdgeView edge)
        {
            _graphCore.DeleteEdgeInternal(edge.GraphCoreEdge);
        }
        

        private void OnUndoRedo()
        {
            Refresh(_graphCore);
        }
    }

}
