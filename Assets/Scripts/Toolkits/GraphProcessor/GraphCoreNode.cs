using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayArk.GraphCore.Data
{
    public class GraphCoreNode : ScriptableObject
    {
        [SerializeField] private string _title = "New State";

        //这里原先写成了属性 并覆盖了无参构造
        //有两个问题：
        //1.Unity的只读属性产生的后端字段是readonly的
        //  但是Unity的序列化器 目前无法对readonly字段进行序列化
        //2.Unity的反序列化通常会绕过构造函数 或需要一个无参构造函数
        //  如果只有带参构造函数 某些情况下会导致数据无法正常恢复

        [field: HideInInspector, SerializeField]
        public string UniqueID { get; private set; }
        [field: HideInInspector, SerializeField]
        public Vector2 ViewPosition { get; private set; }

        /// <summary>
        /// 外部初始化
        /// </summary>
        public void Initialize(string title, Vector2 viewPosition)
        {
            _title = title;
            UniqueID = System.Guid.NewGuid().ToString();
            ViewPosition = viewPosition;
        }
    }
}