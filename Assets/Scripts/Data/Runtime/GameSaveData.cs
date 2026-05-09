using System;
using System.Collections.Generic;
using UnityEngine;

namespace DreamSenker.Data.Runtime
{
[Serializable]
public class GameSaveData : IRunningData
{
    [Header("PLAYER DETAILS")]
    [Tooltip("玩家最近的存档点")]
    public string SavePointID;//存档时设置
    [Tooltip("玩家存档点所在的地图")]
    public string SaveMapId;

    [HideInInspector] public string CurrentMapId;
    [HideInInspector] public string PreviousMapId;

    /// <summary>
    /// 存储所有玩家已经触发过的对话信息
    /// </summary>
    [HideInInspector] public List<string> TriggeredDialogueGuidList;

    [Tooltip("是否遇到了蜘蛛Boss")]
    public bool IsMetSpiderBoss;
    [Tooltip("是否击杀了蜘蛛Boss ------ 获得切换形态的能力")]
    public bool IsKilledSpiderBoss;
    [Tooltip("是否遇到了狐狸Boss")]
    public bool IsMetFoxBoss;
    [Tooltip("是否击杀了狐狸Boss ------ 游戏通关（这个可以不要 击杀后强制通关）")]
    public bool IsKilledFoxBoss;
    [Tooltip("是否拾取了晨露梦核")]
    public bool IsGotChen;
    [Tooltip("是否通关了梦境地图")]
    public bool IsClearMirrorMap;


    public bool CheckDialogueTriggered(string dialogueInfoGuid)
    {
        if (TriggeredDialogueGuidList.Contains(dialogueInfoGuid))
        {
            return true;
        }
        return false;
    }
}
}
