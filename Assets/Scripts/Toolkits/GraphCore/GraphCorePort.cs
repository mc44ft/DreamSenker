
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
        [HideInInspector, SerializeField]
        private string _uniqueID;
        [SerializeField]
        private string _portName = "New Port";
        [HideInInspector, SerializeField]
        private Direction _direction;//端口类型
        [HideInInspector, SerializeField]
        private string _selfNodeID;
        public void Initialize(string portID, string selfNodeID, Direction direction)
        {
            _uniqueID = portID;
            _selfNodeID = selfNodeID;
            _direction = direction;
        }
        public void SetPortName(string portName)
        {
            _portName = portName;
        }

        public string GetUniqueID()
            => _uniqueID;
        public string GetPortName()
            => _portName;
        public Direction GetDirection()
            => _direction;
        public string GetSeleNodeID()
            => _selfNodeID;
    }
}