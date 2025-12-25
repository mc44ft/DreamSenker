
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    /// <summary>
    /// 因为Unity反序列化时 会调用构造函数来new一个数据类再将值拷贝回去
    /// 所以即便是单纯的数据类 也不用构造函数进行初始化
    /// </summary>
    [Serializable]
    public class GraphCorePort
    {
        [field: SerializeField]
        public string UniqueID { get; private set; }
        [field: SerializeField]
        public string PortName { get; private set; } = "New Port";
        [field: SerializeField]
        public Direction Direction { get; private set; }//端口类型
        [field: SerializeField]
        public string SelfNodeID {  get; private set; }
        public void Initialize(string portID, string selfNodeID, Direction direction)
        {
            UniqueID = portID;
            SelfNodeID = selfNodeID;
            Direction = direction;
        }
        public void SetPortName(string portName)
        {
            PortName = portName;
        }
    }
}