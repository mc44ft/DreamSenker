using MapSystem.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class MapExit : MonoBehaviour
{
    //根据这个方向 查找连接的地图
    //在触发器中切换地图，并记录玩家的操作信息，决定下一地图的玩家落点测试
    [Tooltip("关卡出口方向")]
    public E_ExitType ConnectionType;
    [Tooltip("是否是隐藏出口")]
    public bool IsHiddenExit = false;
    [Tooltip("Loading图")]
    [SerializeField] private Sprite _loadingSprite;
    [Tooltip("Loading时间")]
    [SerializeField] private float _loadingTime = 0.5f;

    private BoxCollider2D m_boxCollider2D;
    private void Awake() 
    {
        m_boxCollider2D = GetComponent<BoxCollider2D>();
        m_boxCollider2D.isTrigger = true;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            SceneTransition.Instance.ResetLoading(_loadingSprite, _loadingTime);
            //玩家进入出口 触发事件
            EventCenter.Instance.EventTrigger(E_EventType.Map_ExitTrigger, this, new MapExitTriggerEventArgs(ConnectionType));
        }
        
    }
}
