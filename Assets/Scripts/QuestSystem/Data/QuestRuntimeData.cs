using System;
using UnityEngine;

namespace DreamSenker.QuestSystem.Data
{
/// <summary>
/// 单个任务的运行时存档数据。
/// </summary>
[Serializable]
public class QuestRuntimeData
{
    /// <summary>
    /// 对应 QuestDefinitionSO 的稳定任务 ID。
    /// </summary>
    public string QuestId;
    /// <summary>
    /// 当前任务长期状态。
    /// </summary>
    public EQuestState State;

    /// <summary>
    /// 存档反序列化使用的无参构造。
    /// </summary>
    public QuestRuntimeData() { }

    /// <summary>
    /// 创建一条任务存档数据。
    /// </summary>
    public QuestRuntimeData(string questId, EQuestState state)
    {
        QuestId = questId;
        State = state;
    }

    /// <summary>
    /// 修改任务长期状态。
    /// </summary>
    public void SetState(EQuestState state)
    {
        State = state;
    }
}
}
