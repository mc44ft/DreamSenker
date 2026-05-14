using System;
using System.Collections.Generic;
using PlayArk.GraphCore.Data;
using UnityEditor;
using UnityEngine;

namespace PlayArk.GraphCore
{
    public abstract class GraphCoreGraph : ScriptableObject
    {
        //给表现层返回具体的数据基类
        //给业务层返回泛型
        public abstract IEnumerable<GraphCoreNode> GetNodesInternal();
        public abstract IEnumerable<GraphCoreEdge> GetEdgesInternal();
        public abstract GraphCoreNode CreateNodeInternal(Type type, Vector2 viewPosition);
        public abstract void AddNodeInternal(GraphCoreNode node);
        public abstract void DeleteNodeInternal(GraphCoreNode node);
        public abstract GraphCoreEdge CreateEdgeInternal(string rootNodeID, string rootPortID, string trueNodeID, string truePortID);
        public abstract void AddEdgeInternal(GraphCoreEdge edge);
        public abstract void DeleteEdgeInternal(GraphCoreEdge edge);
    }
}

namespace PlayArk.GraphCore.Data
{
    public class GraphCoreGraph<TNode, TEdge> : GraphCoreGraph, ISerializationCallbackReceiver
        where TNode : GraphCoreNode 
        where TEdge : GraphCoreEdge, new()
    {
        [SerializeField]
        protected List<TNode> _nodes = new();
        [SerializeField]
        protected List<TEdge> _edges = new();

        protected readonly Dictionary<string, TNode> _nodeLookup = new();
        protected readonly Dictionary<string, TEdge> _edgeLookup = new();
        protected readonly Dictionary<string, TEdge> _portToEdgeLookup = new();//通过端口id查找该端口上的连线

        public override IEnumerable<GraphCoreNode> GetNodesInternal() 
            => GetNodes();
        public override IEnumerable<GraphCoreEdge> GetEdgesInternal() 
            => GetEdges();
        public override GraphCoreNode CreateNodeInternal(Type type, Vector2 viewPosition) 
            => CreateNode(type, viewPosition);
        public override void AddNodeInternal(GraphCoreNode node)
            => AddNode(node as TNode);
        public override void DeleteNodeInternal(GraphCoreNode node) 
            => DeleteNode(node as TNode);
        public override GraphCoreEdge CreateEdgeInternal(string rootNodeID, string rootPortID, string trueNodeID, string truePortID)
            => CreateEdge(rootNodeID, rootPortID, trueNodeID, truePortID);
        public override void AddEdgeInternal(GraphCoreEdge edge)
            => AddEdge(edge as TEdge);
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
        /// <summary>
        /// //懒加载 主要是用到字典的时候加载
        /// </summary>
        private void RebuildLookups()
        {
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
        }
        public TNode CreateNode(Type type, Vector2 viewPosition)
        {
            TNode node = CreateInstance(type) as TNode;
            node.Init(Guid.NewGuid().ToString(), viewPosition);
            return node;
        }
        public void AddNode(TNode node)
        {
            _nodes.Add(node);
        }
        public void DeleteNode(TNode node)
        {
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
                _edges.Remove(edge);
            _nodes.Remove(node);
            //Unity保存的时候会保存所有标记为脏的资源 如果没有标记为脏 会不进行保存
            //true是显示授权Unity销毁该资源对象 如果不传true Unity会认为你误操作 会报错
            //UnityEngine.Object.DestroyImmediate(node, true);
        }
        public TEdge CreateEdge(string rootNodeID, string rootPortID, string trueNodeID, string truePortID)
        {
            TEdge edge = new TEdge();
            edge.Initialize(Guid.NewGuid().ToString(), rootNodeID, rootPortID, trueNodeID, truePortID);
            return edge;
        }
        public void AddEdge(TEdge edge)
        {
            _edges.Add(edge);
        }
        public void DeleteEdge(TEdge edge)
        {
            _edges.Remove(edge);
        }

        private void OnValidate()
        {
            //防御性重构字典
            RebuildLookups();
        }
        protected virtual void OnCreateDefaultNode(){}
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
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                return;
            
            //在Project里创建新资源时
            //1.内存中先生成一个对象实例
            //2.Unity提示输入文件名
            //3.按下回车后 文件正式在硬盘中生成（这个函数在文件在硬盘中序列化的前一刻执行， 这时候资源路径已经确定）
            //所以可以在这时进行默认节点的创建（如果有的话）
            //这也是为了实现“自动修复机制” ： 即保证该资源永远时完整的 不会因为误操作而崩溃
            //生成默认节点
            OnCreateDefaultNode();
            
            foreach (var node in _nodes)
            {
                if (node == null) continue;
                //跟上面一样 这里的意思就是：
                //确保该子状态不是一个已经被保存在硬盘里的资源
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(node)))
                {
                    AssetDatabase.AddObjectToAsset(node, this);
                }
            }
#endif
        }
        /// <summary>
        /// 在Unity把保存到磁盘上的二进制数据重新变成内存中的对象之后 立即执行
        /// </summary>
        public void OnAfterDeserialize() { }
    }
}