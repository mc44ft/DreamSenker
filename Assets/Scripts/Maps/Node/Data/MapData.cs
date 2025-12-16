using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MapData
{
    [Header("LEVEL BASIC DETAILS")]
    [Tooltip("仅用于标识的地图名称 无实际逻辑作用")]
    public string MapName;
    [Tooltip("每个场景的玩家遮罩程度")]
    [Range(0f, 200f)]
    public float PlayerShadowDarknessStrength = 2.5f;
    //被自动填写的场景名称
    [HideInInspector] public string MapSceneName;
#if UNITY_EDITOR
    [Tooltip("该项只用于在编辑器时拖拽自动填写场景名称")]
    public SceneAsset SceneAsset;
#endif
    //[Tooltip("是否为初始地图")]
    //public bool 
    
    //[Space(5)]
    //[Header("PLAYER DETAILS")]
    //[Tooltip("玩家在不同方向的出生点")]
    //public Vector2 EastBrithPosition;
    //public Vector2 SouthBrithPosition;
    //public Vector2 WestBrithPosition;
    //public Vector2 NorthBrithPosition;
}
