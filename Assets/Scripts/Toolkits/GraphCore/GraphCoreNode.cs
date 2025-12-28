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

        public abstract void Init(string uniqueID, Vector2 viewPosition);
        public abstract void SetPosition(Vector2 viewPosition);

        
        public abstract GraphCorePort CreatePortInternal(Direction direction);
        public abstract void DeletePortInternal(Direction direction);
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

        [HideInInspector, SerializeField]
        private string _uniqueID;//唯一ID
        [HideInInspector, SerializeField]
        private Vector2 _viewPosition;//编辑器视图坐标 

        [SerializeField]
        protected List<TPort> _inputPorts = new List<TPort>();
        [SerializeField]
        protected List<TPort> _outputPorts = new List<TPort>();

        private Dictionary<string, TPort> _portLookup = new();



        //------------------------------  -----------------------------
        public override string GetUniqueID()
            => _uniqueID;
        public override Vector2 GetViewPosition()
            => _viewPosition;
        public override IEnumerable<GraphCorePort> GetInputPorts()
            => _inputPorts;
        public override IEnumerable<GraphCorePort> GetOutputPorts()
            => _outputPorts;
        public override GraphCorePort CreatePortInternal(Direction direction)
            => CreatePort(direction);
        public override void DeletePortInternal(Direction direction)
            => DeletePort(direction);
        public override GraphCorePort GetPortDataInternal(string portID)
            => GetPortByID(portID);

        
        public TPort GetPortByID(string portID)
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
        public override void Init(string uniqueID, Vector2 viewPosition)
        {
            _uniqueID = uniqueID;
            _viewPosition = viewPosition;

            if (_inputPorts.Count < 1)
                CreatePort(Direction.Input);
            if (_outputPorts.Count < 1)
                CreatePort(Direction.Output);

            EditorUtility.SetDirty(this);
        }

        public TPort CreatePort(Direction direction)
        {
            TPort portData = new TPort();
            portData.Initialize(Guid.NewGuid().ToString(), _uniqueID, direction);
            if (direction == Direction.Input)
                _inputPorts.Add(portData);
            else
                _outputPorts.Add(portData);

            EditorUtility.SetDirty(this);
            return portData;
        }
        public void DeletePort(Direction direction)
        {
            if(direction == Direction.Input && _inputPorts.Count > 0)
                _inputPorts.RemoveAt(_inputPorts.Count - 1);
            else
                _outputPorts.RemoveAt(_outputPorts.Count - 1);
        }
        public override void SetPosition(Vector2 viewPosition)
        {
            Undo.RecordObject(this, "你刚刚移动了一个GraphCoreNode");
            _viewPosition = viewPosition;
            EditorUtility.SetDirty(this);
        }
        protected virtual void OnValidate()
        {
            _portLookup.Clear();
            foreach (var portData in _inputPorts)
            {
                if (portData != null)
                {
                    _portLookup[portData.GetUniqueID()] = portData;
                }
            }
            foreach (var portData in _outputPorts)
            {
                if (portData != null)
                {
                    _portLookup[portData.GetUniqueID()] = portData;
                }
            }
        }
#endif

    }
}