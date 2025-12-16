using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace MapSystem.Nodes
{
    public class MapNode : ScriptableObject
    {
        //在字段初始化器中初始化 防止空引用报错
        public MapData MapData = new MapData();

        public ViewData ViewData = new ViewData();
        public Connections Connections = new Connections();
        public string guid { get; private set; }

        private void OnEnable()
        {
            if(string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                //标记为脏 在按下Ctrl+s时会保存
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }
        /// <summary>
        /// 根据出口类型获取连接的节点
        /// </summary>
        /// <returns></returns>
        public MapNode GetConnectionNodeFromOutputPort(E_ExitType exitType)
        {
            MapNode result = null;
            switch (exitType)
            {
                case E_ExitType.East:
                case E_ExitType.EastHidden:
                    result = Connections.RightOutputConnectionNode;
                    break;
                case E_ExitType.West:
                case E_ExitType.WestHidden:
                    result = Connections.LeftOutputConnectionNode;
                    break;
                case E_ExitType.South:
                case E_ExitType.SouthHidden:
                    result = Connections.BottomOutputConnectionNode;
                    break;
                case E_ExitType.North:
                case E_ExitType.NorthHidden:
                    result = Connections.TopOutputConnectionNode;
                    break;
            }
            return result;
        }
        /// <summary>
        /// 存储连接的方法
        /// </summary>
        public void SaveConnection(E_ConnectionType type, MapNode connectionNode)
        {
            switch (type)
            {
                case E_ConnectionType.TopInput:
                    Connections.TopInputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.LeftInput:
                    Connections.LeftInputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.RightInput:
                    Connections.RightInputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.BottomInput:
                    Connections.BottomInputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.TopOutput:
                    Connections.TopOutputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.LeftOutput:
                    Connections.LeftOutputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.RightOutput:
                    Connections.RightOutputConnectionNode = connectionNode;
                    break;
                case E_ConnectionType.BottomOutput:
                    Connections.BottomOutputConnectionNode = connectionNode;
                    break;
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif

        }
        /// <summary>
        /// 删除连接的方法
        /// </summary>
        public void DeleteConnection(E_ConnectionType type)
        {
            switch (type)
            {
                case E_ConnectionType.TopInput:
                    Connections.TopInputConnectionNode = null;
                    break;
                case E_ConnectionType.LeftInput:
                    Connections.LeftInputConnectionNode = null;
                    break;
                case E_ConnectionType.RightInput:
                    Connections.RightInputConnectionNode = null;
                    break;
                case E_ConnectionType.BottomInput:
                    Connections.BottomInputConnectionNode = null;
                    break;
                case E_ConnectionType.TopOutput:
                    Connections.TopOutputConnectionNode = null;
                    break;
                case E_ConnectionType.LeftOutput:
                    Connections.LeftOutputConnectionNode = null;
                    break;
                case E_ConnectionType.RightOutput:
                    Connections.RightOutputConnectionNode = null;
                    break;
                case E_ConnectionType.BottomOutput:
                    Connections.BottomOutputConnectionNode = null;
                    break;
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// 拖入场景资源自动填充 场景名称字段参数信息
        /// </summary>
        private void OnValidate()
        {
            if(MapData.SceneAsset != null)
            {
                MapData.MapSceneName = MapData.SceneAsset.name;
            }
        }
#endif
    }
    //这里给类加上可序列化字段 不是为了让其显示在Inspector中
    //而是为了让Unity知道应该怎么保存该类型的数据
    //Unity在保存SO资源时，只会保存可序列化的字段
    //对于private字段 可以添加[SerializeField]来使其可序列化，可被保存
    //对于public字段 可以添加[System.NonSerialized]使其不被序列化 不被保存
    [Serializable]
    public class ViewData
    {
        public Vector2 Position;//编辑器窗口中的坐标
    }
    [Serializable]
    public class Connections
    {
        //存储输入端口连接的节点
        public MapNode TopInputConnectionNode;
        public MapNode LeftInputConnectionNode;
        public MapNode RightInputConnectionNode;
        public MapNode BottomInputConnectionNode;

        //存储输出端口连接的节点
        public MapNode TopOutputConnectionNode;
        public MapNode LeftOutputConnectionNode;
        public MapNode RightOutputConnectionNode;
        public MapNode BottomOutputConnectionNode;

        /// <summary>
        /// 实现一个只返回inputNode的迭代器对象
        /// </summary>
        public IEnumerable<MapNode> InputNodes
        {
            get
            {
                yield return TopInputConnectionNode;
                yield return LeftInputConnectionNode;
                yield return RightInputConnectionNode;
                yield return BottomInputConnectionNode;
            }
        }
        /// <summary>
        /// 实现一个只返回outputNode的迭代器对象
        /// </summary>
        public IEnumerable<MapNode> OutputNodes
        {
            get
            {
                yield return LeftOutputConnectionNode;
                yield return RightOutputConnectionNode;
                yield return TopOutputConnectionNode;
                yield return LeftOutputConnectionNode;
            }
        }
    }
    public enum E_ConnectionType
    {
        TopInput,
        LeftInput,
        RightInput,
        BottomInput,
        TopOutput,
        LeftOutput,
        RightOutput,
        BottomOutput,
    }
}