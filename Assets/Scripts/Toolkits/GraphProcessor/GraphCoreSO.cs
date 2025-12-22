using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    [CreateAssetMenu(menuName = "PlayArk Assets/GraphCore")]
    public class GraphCoreSO : ScriptableObject
    {
        private List<GraphCoreNode> nodes;
        private Dictionary<string, GraphCoreNode> nodeLookup;

        public GraphCoreNode CreateNode(Type type, Vector2 viewPosition)
        {
            GraphCoreNode node = MakeNode(type, viewPosition);
            AddNode(node);
            return node;
        }
        public IEnumerable<GraphCoreNode> GetNodes()
        {
            return nodes;
        }
        private GraphCoreNode MakeNode(Type type, Vector2 viewPosition)
        {
            GraphCoreNode node = CreateInstance(type) as GraphCoreNode;

            node.Initialize(type.Name, viewPosition);
            return node;
        }
        private void AddNode(GraphCoreNode node)
        {
            nodes.Add(node);
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
            nodeLookup.Clear();

            foreach (var node in nodes)
            {
                //这里是为了防止在编辑器列表中出现的空位导致的空引用
                //有两种清空会导致states里有位置但为null
                //1.手动增加列表长度：在Inspector窗口中手动将列表的长度更改为更长的数值 新的空位就是null
                //2.资产被删除：资产被意外删除，列表原本的位置会显示missing 此时也是null
                if (node != null)
                {
                    nodeLookup[node.UniqueID] = node;
                }
            }

        }

        public void RemoveNode(GraphCoreNode node)
        {
            nodes.Remove(node);
            OnValidate();
        }
    }
}