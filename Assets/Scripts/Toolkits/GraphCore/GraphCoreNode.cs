using System;
using UnityEditor.Experimental.GraphView;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PlayArk.GraphCore.Data
{
    public abstract class GraphCoreNode : ScriptableObject
    {
        public abstract string GetUniqueID();
        public abstract Vector2 GetViewPosition();
        public abstract IEnumerable<GraphCorePort> GetInputPorts();
        public abstract IEnumerable<GraphCorePort> GetOutputPorts();

        public abstract void Initialize(string uniqueID, Vector2 viewPosition);
        public abstract void SetPosition(Vector2 viewPosition);

        
        public abstract GraphCorePort CreatePortInternal(Direction direction);
        public abstract GraphCorePort GetPortDataInternal(string portID);
    }
    public class GraphCoreNode<TPort> : GraphCoreNode where TPort : GraphCorePort, new()
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

        [field: SerializeField]
        public List<TPort> InputPorts { get; private set; } = new List<TPort>();
        [field: HideInInspector, SerializeField]
        public List<TPort> OutputPorts {  get; private set; } = new List<TPort>();

        private Dictionary<string, TPort> _portLookup = new();



        //------------------------------  -----------------------------
        public override string GetUniqueID()
            => UniqueID;
        public override Vector2 GetViewPosition()
            => ViewPosition;
        public override IEnumerable<GraphCorePort> GetInputPorts()
            => InputPorts;
        public override IEnumerable<GraphCorePort> GetOutputPorts()
            => OutputPorts;
        public override GraphCorePort CreatePortInternal(Direction direction)
            => CreatePort(direction);
        public override GraphCorePort GetPortDataInternal(string portID)
            => GetGraphCorePort(portID);

        
        public TPort GetGraphCorePort(string portID)
        {
            if(_portLookup.ContainsKey(portID))
            {
                return _portLookup[portID];
            }
            return null;
        }
#if UNITY_EDITOR
        
        /// <summary>
        /// 外部初始化
        /// </summary>
        public override void Initialize(string uniqueID, Vector2 viewPosition)
        {
            UniqueID = uniqueID;
            ViewPosition = viewPosition;

            if (InputPorts.Count < 1)
                CreatePort(Direction.Input);
            if (OutputPorts.Count < 1)
                CreatePort(Direction.Output);

            EditorUtility.SetDirty(this);
        }

        public TPort CreatePort(Direction direction)
        {
            TPort portData = new TPort();
            portData.Initialize(Guid.NewGuid().ToString(), UniqueID, direction);
            if (direction == Direction.Input)
                InputPorts.Add(portData);
            else
                OutputPorts.Add(portData);

            EditorUtility.SetDirty(this);
            return portData;
        }
        public override void SetPosition(Vector2 viewPosition)
        {
            Undo.RecordObject(this, "你刚刚移动了一个GraphCoreNode");
            ViewPosition = viewPosition;
            EditorUtility.SetDirty(this);
        }
        protected virtual void OnValidate()
        {
            _portLookup.Clear();
            foreach (var portData in InputPorts)
            {
                if (portData != null)
                {
                    _portLookup[portData.UniqueID] = portData;
                }
            }
            foreach (var portData in OutputPorts)
            {
                if (portData != null)
                {
                    _portLookup[portData.UniqueID] = portData;
                }
            }
        }
#endif

    }
}