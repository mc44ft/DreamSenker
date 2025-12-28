using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    public abstract class GraphCoreGraph : ScriptableObject
    {
        //给表现层返回具体的数据基类
        //给业务层返回泛型
        public abstract IEnumerable<GraphCoreNode> GetNodesInternal();
        public abstract IEnumerable<GraphCoreEdge> GetEdgesInternal();
        public abstract GraphCoreNode CreateNodeInternal(Type type, Vector2 viewPosition);
        public abstract void DeleteNodeInternal(GraphCoreNode node);
        public abstract GraphCoreEdge CreateEdgeInternal(string rootNodeID, string rootPortID, string trueNodeID, string truePortID);
        public abstract void DeleteEdgeInternal(GraphCoreEdge edge);
    }
    public class GraphCoreGraph<TNode, TEdge> : GraphCoreGraph, ISerializationCallbackReceiver
        where TNode : GraphCoreNode 
        where TEdge : GraphCoreEdge, new()
    {
        [SerializeField]
        private List<TNode> _nodes = new();
        [SerializeField]
        private List<TEdge> _edges = new();

        private Dictionary<string, TNode> _nodeLookup = new();
        private Dictionary<string, TEdge> _edgeLookup = new();
        private Dictionary<string, TEdge> _portToEdgeLookup = new();//通过端口id查找该端口上的连线

        private bool _isLookupDirty = true;//懒加载标识 

        public override IEnumerable<GraphCoreNode> GetNodesInternal() 
            => GetNodes();
        public override IEnumerable<GraphCoreEdge> GetEdgesInternal() 
            => GetEdges();
        public override GraphCoreNode CreateNodeInternal(Type type, Vector2 viewPosition) 
            => CreateNode(type, viewPosition);
        public override void DeleteNodeInternal(GraphCoreNode node) 
            => DeleteNode(node as TNode);
        public override GraphCoreEdge CreateEdgeInternal(string rootNodeID, string rootPortID, string trueNodeID, string truePortID)
            => CreateEdge(rootNodeID, rootPortID, trueNodeID, truePortID);
        public override void DeleteEdgeInternal(GraphCoreEdge edge)
            => DeleteEdge(edge as TEdge);

        public IEnumerable<TNode> GetNodes()
        {
            return _nodes;
        }
        public IEnumerable<TEdge> GetEdges()
        {
            return _edges;
        }
        public TNode GetNodeByID(string nodeID)
        {
            RebuildLookups();

            if (_nodeLookup.ContainsKey(nodeID))
                return _nodeLookup[nodeID];
            return null;
        }
        public TEdge GetEdgeByID(string edgeID)
        {
            RebuildLookups();

            if (_edgeLookup.ContainsKey(edgeID))
                return _edgeLookup[edgeID];
            return null;
        }
        public TEdge GetEdgeByPortID(string portID)
        {
            RebuildLookups();

            if (_portToEdgeLookup.ContainsKey(portID))
                return _portToEdgeLookup[portID];
            return null;
        }
        private void RebuildLookups()
        {
            //懒加载（如果字典没有脏 则不重构字典）
            if (!_isLookupDirty) 
                return;

            //这里完全清除字典
            //1.是为了提升程序的鲁棒性
            //  全量重建（Clear + ForEach） 是一种极其稳健的“降维打击”手段。
            //  它不关心你做了什么改动，只保证结果：字典里的内容永远和列表一模一样。
            //  这样可以彻底杜绝由于数据同步不及时导致的 NullReferenceException。
            //2.字典不支持原生序列化 当重新从磁盘加载该资源文件时 字典一定是空的
            //  这里通过清空再重新填充 可以确保字典在编辑器环境下始终有数据
            _nodeLookup.Clear();
            _edgeLookup.Clear();
            _portToEdgeLookup.Clear();

            foreach (var node in _nodes)
            {
                //这里是为了防止在编辑器列表中出现的空位导致的空引用
                //有两种清空会导致states里有位置但为null
                //1.手动增加列表长度：在Inspector窗口中手动将列表的长度更改为更长的数值 新的空位就是null
                //2.资产被删除：资产被意外删除，列表原本的位置会显示missing 此时也是null
                if (node == null) continue;
                _nodeLookup[node.GetUniqueID()] = node;
            }
            foreach (var edge in _edges)
            {
                if (edge == null) continue;

                _edgeLookup[edge.UniqueID] = edge;

                _portToEdgeLookup[edge.RootPortID] = edge;
                _portToEdgeLookup[edge.ConnectionPortID] = edge;
            }

            //字典重构完毕 修改标识符
            _isLookupDirty = false;
        }
#if UNITY_EDITOR
        public TNode CreateNode(Type type, Vector2 viewPosition)
        {
            TNode node = MakeNode(type, viewPosition);
            //注册新资源的诞生
            //将刚刚创建的资源对象注册给Undo
            //Ondo会自动将资源标记为脏
            //字符串参数 是这个操作的名称 在Edit选项下会显示这个
            Undo.RegisterCreatedObjectUndo(node, "你刚刚创建了一个GraphCoreNode");
            //记录现有资源的样子
            //将当前StateMachine的快照记录到Undo中 撤销之后就是恢复现在的样子
            //这个字符串的意思 就是刚才这个操作的名称
            Undo.RecordObject(this, "你刚刚添加了一个GraphCoreNode");
            AddNode(node);

            return node;
        }
        private TNode MakeNode(Type type, Vector2 viewPosition)
        {
            TNode node = CreateInstance(type) as TNode;
            node.Init(Guid.NewGuid().ToString(), viewPosition);
            return node;
        }
        private void AddNode(TNode node)
        {
            _nodes.Add(node);
            OnValidate();
        }
        public void DeleteNode(TNode node)
        {
            Undo.RecordObject(this, "你刚刚删除了一个GraphCoreNode");

            //先检查该节点相关的edge
            List<TEdge> edgesToRemove = new List<TEdge>();
            foreach (var edge in _edges)
            {
                if(edge.ConnectionNodeID == node.GetUniqueID() || edge.RootNodeID == node.GetUniqueID())
                {
                    edgesToRemove.Add(edge);
                }
            }
            foreach (var edge in edgesToRemove)
            {
                _edges.Remove(edge);
            }

            _nodes.Remove(node);

            OnValidate();
            //使用Undo操作代替
            Undo.DestroyObjectImmediate(node);

            //Unity保存的时候会保存所有标记为脏的资源 如果没有标记为脏 会不进行保存
            //true是显示授权Unity销毁该资源对象 如果不传true Unity会认为你误操作 会报错
            //UnityEngine.Object.DestroyImmediate(node, true);
        }
        public TEdge CreateEdge(string rootNodeID, string rootPortID, string trueNodeID, string truePortID)
        {
            TEdge edge = new TEdge();
            edge.Initialize(Guid.NewGuid().ToString(), rootNodeID, rootPortID, trueNodeID, truePortID);
            //记录现有资源的样子
            //将当前StateMachine的快照记录到Undo中 撤销之后就是恢复现在的样子
            //这个字符串的意思 就是刚才这个操作的名称
            Undo.RecordObject(this, "你刚刚建立了一个GraphCoreEdge连接");
            AddEdge(edge);
            return edge;
        }
        private void AddEdge(TEdge edge)
        {
            _edges.Add(edge);
            OnValidate();
        }
        public void DeleteEdge(TEdge edge)
        {
            Undo.RecordObject(this, "你刚刚删除了一个GraphCoreEdge");
            _edges.Remove(edge);
            OnValidate();
        }
        private void OnValidate()
        {
            RebuildLookups();
        }
#endif

        /// <summary>
        /// 在序列化的前一刻调用
        /// 在以下几种情况触发：
        /// 1.手动保存时 control + s
        /// 2.自动保存
        /// 3.打包
        /// 4.Play
        /// 5.代码编译后
        /// 在Unity把对象数据写入文件之前触发（从内存到硬盘）
        /// </summary>
        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            //这里确保该状态机主资源已经是一个保存在硬盘当中的资源了
            //这里的意思就是获取这个类所对应的资源在硬盘中的路径
            //如果该路径不为空 说明这个类对应的资源已经保存在硬盘当中了
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
            {
                foreach (var node in _nodes)
                {
                    //跟上面一样 这里的意思就是：
                    //确保该子状态不是一个已经被保存在硬盘里的资源
                    if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(node)))
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }

            //在Project里创建新资源时
            //1.内存中先生成一个对象实例
            //2.Unity提示输入文件名
            //3.按下回车后 文件正式在硬盘中生成（这个函数在文件在硬盘中序列化的前一刻执行， 这时候资源路径已经确定）
            //所以可以在这时进行默认节点的创建（如果有的话）
            //这也是为了实现“自动修复机制” ： 即保证该资源永远时完整的 不会因为误操作而崩溃
#endif
        }
        /// <summary>
        /// 在Unity把保存到磁盘上的二进制数据重新变成内存中的对象之后 立即执行
        /// </summary>
        public void OnAfterDeserialize()
        {
            //游戏运行时重建
            //编辑器加载时重建
            //这里直接重建字典 可能这个时候数据还没有完全恢复，所以出现了报错，改为懒加载
            //RebuildLookups();
            //这里不再选择直接重构字典
            //改为修改标识符
            _isLookupDirty = true;
        }
    }
}