using System;
using UnityEngine;

namespace PlayArk.GraphCore
{
    [Serializable]
    public class GraphCoreEdge
    {
        [SerializeField] private string _uniqueID;
        [SerializeField] private string _rootNodeID;
        [SerializeField] private string _rootPortID;
        [SerializeField] private string _connectionNodeID;
        [SerializeField] private string _connectionPortID;
        
        public string UniqueID => _uniqueID;
        public string RootNodeID => _rootNodeID;
        public string RootPortID => _rootPortID;
        public string ConnectionNodeID => _connectionNodeID;
        public string ConnectionPortID => _connectionPortID;

        public void Initialize(string uniqueID, string rootNodeID, string rootPortID, string connectionNodeID, string connectionPortID)
        {
            _uniqueID = uniqueID; 
            _rootNodeID = rootNodeID;
            _rootPortID = rootPortID;
            _connectionNodeID = connectionNodeID;
            _connectionPortID = connectionPortID;
        }

    }
}