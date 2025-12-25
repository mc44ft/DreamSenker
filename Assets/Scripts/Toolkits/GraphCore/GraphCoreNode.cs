using System;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PlayArk.GraphCore.Data
{
    public class GraphCoreNode : ScriptableObject
    {
        //屏蔽字段未使用的警告
#pragma warning disable CS0414
        [SerializeField] 
        private string _title = "New Node";
#pragma warning restore CS0414

        //这里原先写成了属性 并覆盖了无参构造
        //有两个问题：
        //1.Unity的只读属性产生的后端字段是readonly的
        //  但是Unity的序列化器 目前无法对readonly字段进行序列化
        //2.Unity的反序列化通常会绕过构造函数 或需要一个无参构造函数
        //  如果只有带参构造函数 某些情况下会导致数据无法正常恢复

        [field: HideInInspector, SerializeField]
        public string UniqueID { get; private set; }//唯一ID
        [field: HideInInspector, SerializeField]
        public Vector2 ViewPosition { get; private set; }//编辑器视图坐标 

        [field: HideInInspector, SerializeField]
        public List<GraphCorePort> InputPorts { get; private set; } = new List<GraphCorePort>();
        [field: HideInInspector, SerializeField]
        public List<GraphCorePort> OutputPorts {  get; private set; } = new List<GraphCorePort>();

        private Dictionary<string, GraphCorePort> _portLookup = new();
        /// <summary>
        /// 外部初始化
        /// </summary>
        public void Initialize(string uniqueID, Vector2 viewPosition)
        {
            UniqueID = uniqueID;
            ViewPosition = viewPosition;

            if (InputPorts.Count < 1)
                CreatePort(Direction.Input);
            if (OutputPorts.Count < 1)
                CreatePort(Direction.Output);
        }
        
        public GraphCorePort CreatePort(Direction direction)
        {
            GraphCorePort portData = new GraphCorePort();
            portData.Initialize(Guid.NewGuid().ToString(), UniqueID, direction);
            if(direction == Direction.Input)
                InputPorts.Add(portData);
            else
                OutputPorts.Add(portData);

            EditorUtility.SetDirty(this);
            return portData;
        }
        public GraphCorePort GetPortData(string portID)
        {
            if(_portLookup.ContainsKey(portID))
            {
                return _portLookup[portID];
            }
            return null;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            _portLookup.Clear();
            foreach (var portData in InputPorts)
            {
                if(portData != null)
                {
                    _portLookup[portData.UniqueID] = portData;
                }
            }
            foreach (var portData in OutputPorts)
            {
                if(portData != null)
                {
                    _portLookup[portData.UniqueID] = portData;
                }
            }
        }
        public void SetPosition(Vector2 viewPosition)
        {
            Undo.RecordObject(this, "你刚刚移动了一个GraphCoreNode");
            ViewPosition = viewPosition;
            EditorUtility.SetDirty(this);
        }
#endif

    }
}