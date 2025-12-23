using PlayArk.GraphCore.Data;
using System;
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

        private GraphCoreSO _graphCore;

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
        }
        /// <summary>
        /// 根据GraphCoreSO的数据刷新画布
        /// 切换资源时 或者 更新当前资源时都会调用
        /// 撤销重做时
        /// </summary>
        public void Refresh(GraphCoreSO graphCore)
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
                foreach (var node in _graphCore.GetNodes())
                {
                    CreateNodeView(node);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            //被删除的元素
            var elementsToRemove = graphViewChange.elementsToRemove;
            if(elementsToRemove != null)
            {
                foreach (var element in elementsToRemove)
                {
                    if (element is GraphCoreNodeView nodeView)
                    {
                        RemoveNode(nodeView);
                    }
                }
            }
            return graphViewChange;
        }

        

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (!Application.isPlaying)
            {
                base.BuildContextualMenu(evt);

                //解决画布缩放拖拽和屏幕尺寸不同步的问题
                Vector2 mousePosition = 
                    viewTransform.matrix.//代表了当前画布的状态
                    inverse.//逆矩阵
                    MultiplyPoint(evt.mousePosition);//矩阵计算

                //添加菜单项
                evt.menu.AppendAction("Carate Node", a => CreateNode(typeof(GraphCoreNode), mousePosition));
            }
            

        }
        private void CreateNode(Type nodeType, Vector2 mousePosition)
        {
            GraphCoreNode node = _graphCore.CreateNode(nodeType, mousePosition);
            CreateNodeView(node);
        }
        private void RemoveNode(GraphCoreNodeView nodeView)
        {
            _graphCore.RemoveNode(nodeView.CoreNode);
        }
        private void CreateNodeView(GraphCoreNode node)
        {
            GraphCoreNodeView nodeView = new GraphCoreNodeView(node);
            AddElement(nodeView);
        }
    }

}
