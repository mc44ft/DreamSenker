using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    [CreateAssetMenu(menuName = "PlayArk Assets/GraphCore")]
    public class GraphCoreSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<GraphCoreNode> _nodes = new();
        [SerializeField]
        private List<GraphCoreEdge> _edges = new();
        private Dictionary<string, GraphCoreNode> _nodeLookup = new();
        private Dictionary<string, GraphCoreEdge> _edgeLookup = new();
        
        public IEnumerable<GraphCoreNode> GetNodes()
        {
            return _nodes;
        }
        public IEnumerable<GraphCoreEdge> GetEdges()
        {
            return _edges;
        }
        public GraphCoreNode GetNodeByID(string nodeID)
        {
            if(_nodeLookup.ContainsKey(nodeID))
            {
                return _nodeLookup[nodeID];
            }
            return null;
        }
        public GraphCoreEdge GetEdgeByID(string edgeID)
        {
            if(_edgeLookup.ContainsKey(edgeID))
            {
                return _edgeLookup[edgeID];
            }
            return null;
        }
        
#if UNITY_EDITOR
        public GraphCoreNode CreateNode(Type type, Vector2 viewPosition)
        {
            GraphCoreNode node = MakeNode(type, viewPosition);
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
        private GraphCoreNode MakeNode(Type type, Vector2 viewPosition)
        {
            GraphCoreNode node = CreateInstance(type) as GraphCoreNode;
            node.Initialize(Guid.NewGuid().ToString(), viewPosition);
            return node;
        }
        private void AddNode(GraphCoreNode node)
        {
            _nodes.Add(node);
            OnValidate();
        }
        public void DeleteNode(GraphCoreNode node)
        {
            Undo.RecordObject(this, "你刚刚删除了一个GraphCoreNode");

            //先检查该节点相关的edge
            List<GraphCoreEdge> edgesToRemove = new List<GraphCoreEdge>();
            foreach (var edge in _edges)
            {
                if(edge.ConnectionNodeID == node.UniqueID || edge.RootNodeID == node.UniqueID)
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
        public GraphCoreEdge CreateEdge(string rootNodeID, string rootPortID, string trueNodeID, string truePortID)
        {
            GraphCoreEdge edge = new GraphCoreEdge();
            edge.Initialize(Guid.NewGuid().ToString(), rootNodeID, rootPortID, trueNodeID, truePortID);
            //记录现有资源的样子
            //将当前StateMachine的快照记录到Undo中 撤销之后就是恢复现在的样子
            //这个字符串的意思 就是刚才这个操作的名称
            Undo.RecordObject(this, "你刚刚建立了一个GraphCoreEdge连接");
            AddEdge(edge);
            return edge;
        }
        private void AddEdge(GraphCoreEdge edge)
        {
            _edges.Add(edge);
            OnValidate();
        }
        public void DeleteEdge(GraphCoreEdge edge)
        {
            Undo.RecordObject(this, "你刚刚删除了一个GraphCoreEdge");
            _edges.Remove(edge);
            OnValidate();
        }
        private void OnValidate()
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

            foreach (var node in _nodes)
            {
                //这里是为了防止在编辑器列表中出现的空位导致的空引用
                //有两种清空会导致states里有位置但为null
                //1.手动增加列表长度：在Inspector窗口中手动将列表的长度更改为更长的数值 新的空位就是null
                //2.资产被删除：资产被意外删除，列表原本的位置会显示missing 此时也是null
                if (node != null)
                {
                    _nodeLookup[node.UniqueID] = node;
                }
            }
            foreach (var edge in _edges)
            {
                if(edge != null)
                {
                    _edgeLookup[edge.UniqueID] = edge;
                }
            }
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

        public void OnAfterDeserialize()
        {
            
        }
    }
}