
using PlayArk.GraphCore.Data;
using System.Linq;
using PlayArk.GraphCore.Utilities;
using UnityEngine;
namespace MapSystem.Nodes
{
    [NodeMenuItem("MapNode")]
    public class MapNode : GraphCoreNode<MapGraphPort>
    {
        //在字段初始化器中初始化 防止空引用报错
        public MapData MapData = new MapData();


        public MapGraphPort GetMapGraphPortByEnum(E_ExitType exitType)
        {
            return _inputPorts.Where(port => port.ExitType == exitType).FirstOrDefault();
        }
        public MapGraphPort GetMapGraphPortByEnum(E_SpawnType spawnType) 
        {
            return _inputPorts.Where(port => port.SpawnType == spawnType).FirstOrDefault();
        }
#if UNITY_EDITOR
        /// <summary>
        /// 拖入场景资源自动填充 场景名称字段参数信息
        /// </summary>
        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (MapData.SceneAsset != null)
            {
                MapData.MapSceneName = MapData.SceneAsset.name;
            }
        }
#endif
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