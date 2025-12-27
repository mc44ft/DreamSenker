using System;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    [Serializable]
    public class GraphCoreEdge
    {
        [field: SerializeField]
        public string UniqueID { get; private set; }
        [field: SerializeField]
        public string RootNodeID { get; private set; }
        [field: SerializeField]
        public string RootPortID { get; private set; }
        [field: SerializeField]
        public string ConnectionNodeID { get; private set; }
        [field: SerializeField]
        public string ConnectionPortID { get; private set; }


        public void Initialize(string uniqueID, string rootNodeID, string rootPortID, string connectionNodeID, string connectionPortID)
        {
            UniqueID = uniqueID; 
            RootNodeID = rootNodeID;
            RootPortID = rootPortID;
            ConnectionNodeID = connectionNodeID;
            ConnectionPortID = connectionPortID;
        }

    }
}