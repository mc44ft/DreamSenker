using MapSystem.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace MapSystem.Graph
{
    [CreateAssetMenu(fileName = "MapGraph_", menuName = "NodeGraph/MapGraph")]
    public class MapGraph : ScriptableObject
    {
        //在SO资源被创建出来的时候就是有值的
        public List<MapNode> Nodes = new List<MapNode>();
        public string guid {  get; private set; }


        public MapNode GetMapNodeFromGuid(string guid)
        {
            return Nodes.Where(node => node.guid == guid).FirstOrDefault();
        }

#if UNITY_EDITOR
        public MapNode CreateMapNode()
        {
            //创建一个node资产在内存中
            //此操作不会调用构造函数 但会调用OnEnable方法
            //所以将创建GUID的逻辑放在OnEnable方法中
            MapNode node = ScriptableObject.CreateInstance<MapNode>();

            //将其加入图中
            Nodes.Add(node);
            //添加到图资源中
            AssetDatabase.AddObjectToAsset(node, this);

            //保存资源
            AssetDatabase.SaveAssets();
            return node;
        }
        public MapNode DeleteNode(MapNode node)
        {
            Nodes.Remove(node);
            //从图资源中删除
            AssetDatabase.RemoveObjectFromAsset(node);

            //保存资源
            AssetDatabase.SaveAssets();

            return node;
        }
        /// <summary>
        /// 保存端口连线信息
        /// </summary>
        public void SaveConnection(MapNode outputNode, E_ConnectionType outputType, MapNode inputNode, E_ConnectionType inputType)
        {
            //保存两端的连线信息
            outputNode.SaveConnection(outputType, inputNode);
            inputNode.SaveConnection(inputType, outputNode);
        }
        /// <summary>
        /// 删除端口连线信息
        /// </summary>
        public void DeleteConnection(MapNode outputNode, E_ConnectionType outputType, MapNode inputNode, E_ConnectionType inputType)
        {
            outputNode.DeleteConnection(outputType);
            inputNode.DeleteConnection(inputType);
        }
        /// <summary>
        /// 在右键创建资产或者Unity编译时 这个方法都会被调用
        /// </summary>
        private void OnEnable()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}

