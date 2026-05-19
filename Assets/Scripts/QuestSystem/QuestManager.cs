using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using DreamSeeker.Data.Runtime;
using DreamSeeker.Inventory;
using DreamSeeker.Managers;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.QuestSystem
{
/// <summary>
/// 任务运行时入口，统一处理任务状态、接取、可交付判断和交付。
/// </summary>
public class QuestManager : BaseManager<QuestManager>
{
    /// <summary>
    /// 任务固定配置索引。
    /// </summary>
    private readonly Dictionary<string, QuestDefinitionSO> _questDefinitionDict = new Dictionary<string, QuestDefinitionSO>();
    /// <summary>
    /// 任务存档数据列表，直接引用 GameSaveData 中的数据源。
    /// </summary>
    private List<QuestRuntimeData> _runtimeDataList;

    private QuestManager() { }

    /// <summary>
    /// 注入任务配置和任务存档数据。
    /// </summary>
    public void SetupData(QuestListSO questList, List<QuestRuntimeData> runtimeDataList)
    {
        _questDefinitionDict.Clear();
        _runtimeDataList = runtimeDataList;

        if (_runtimeDataList == null)
        {
            Debug.LogError("QuestManager SetupData 失败：任务存档列表为空");
            return;
        }

        if (questList == null)
        {
            Debug.LogWarning("QuestManager SetupData：任务配置列表为空");
            return;
        }

        foreach (QuestDefinitionSO questDefinition in questList.Quests)
        {
            if (questDefinition == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(questDefinition.QuestId))
            {
                Debug.LogError($"任务配置 {questDefinition.name} 的 QuestId 为空");
                continue;
            }

            if (_questDefinitionDict.ContainsKey(questDefinition.QuestId))
            {
                Debug.LogError($"重复的 QuestId：{questDefinition.QuestId}");
                continue;
            }

            _questDefinitionDict.Add(questDefinition.QuestId, questDefinition);
        }
    }

    /// <summary>
    /// 查询任务固定配置。
    /// </summary>
    public bool TryGetQuestDefinition(string questId, out QuestDefinitionSO questDefinition)
    {
        questDefinition = null;
        return !string.IsNullOrWhiteSpace(questId) &&
               _questDefinitionDict.TryGetValue(questId, out questDefinition);
    }

    /// <summary>
    /// 接取任务，只有未接取状态可以变为进行中。
    /// </summary>
    public bool StartQuest(string questId)
    {
        if (!TryGetQuestDefinitionOrLog(questId, out _))
        {
            return false;
        }

        QuestRuntimeData runtimeData = GetOrCreateRuntimeData(questId);
        if (runtimeData.State != EQuestState.NotStarted)
        {
            Debug.LogWarning($"任务接取失败：当前状态不允许接取，QuestId={questId}, State={runtimeData.State}");
            return false;
        }

        runtimeData.SetState(EQuestState.Active);
        return true;
    }

    public void TrackQuest(string questId)
    {
        if (TryGetQuestDefinitionOrLog(questId, out QuestDefinitionSO quest))
        {
            GameManager.Instance.GameSaveData.CurrentTrackQuestID = questId;
            EventCenter.Instance.EventTrigger(E_EventType.Quest_TrackChanged, this,  new StringEventArgs(questId));
        }
    }
    /// <summary>
    /// 判断任务当前是否可交付。
    /// </summary>
    public bool CanCompleteQuest(string questId)
    {
        if (!TryGetQuestDefinitionOrLog(questId, out QuestDefinitionSO questDefinition))
        {
            return false;
        }

        if (GetQuestState(questId) != EQuestState.Active)
        {
            return false;
        }

        QuestItemRequirement[] requirements = questDefinition.ItemRequirements;
        if (requirements == null || requirements.Length == 0)
        {
            return true;
        }

        return requirements.All(requirement =>
            requirement != null &&
            InventoryManager.Instance.HasItem(requirement.ItemType, requirement.Amount));
    }

    /// <summary>
    /// 完成任务并扣除交付物品。
    /// </summary>
    public bool CompleteQuest(string questId)
    {
        if (!TryGetQuestDefinitionOrLog(questId, out QuestDefinitionSO questDefinition))
        {
            return false;
        }

        QuestRuntimeData runtimeData = GetOrCreateRuntimeData(questId);
        if (runtimeData.State != EQuestState.Active)
        {
            Debug.LogWarning($"任务交付失败：当前状态不允许交付，QuestId={questId}, State={runtimeData.State}");
            return false;
        }

        if (!CanCompleteQuest(questId))
        {
            Debug.LogWarning($"任务交付失败：交付条件不满足，QuestId={questId}");
            return false;
        }

        QuestItemRequirement[] requirements = questDefinition.ItemRequirements;
        if (requirements != null)
        {
            foreach (QuestItemRequirement requirement in requirements)
            {
                if (requirement == null)
                {
                    continue;
                }

                InventoryManager.Instance.TryRemoveItem(requirement.ItemType, requirement.Amount);
            }
        }

        runtimeData.SetState(EQuestState.Completed);
        return true;
    }

    public List<QuestDefinitionSO> GetAllRunningQuests()
    {
        List<QuestDefinitionSO> runningQuests = new List<QuestDefinitionSO>();
        foreach (QuestRuntimeData runtimeData in _runtimeDataList)
        {
            if (runtimeData.State == EQuestState.Active &&
                TryGetQuestDefinition(runtimeData.QuestId, out QuestDefinitionSO questDefinition))
            {
                runningQuests.Add(questDefinition);
            }
        }
        return runningQuests;
    }
    /// <summary>
    /// 获取任务长期状态；没有存档记录时视为未接取。
    /// </summary>
    public EQuestState GetQuestState(string questId)
    {
        if (string.IsNullOrWhiteSpace(questId) || _runtimeDataList == null)
        {
            return EQuestState.NotStarted;
        }

        QuestRuntimeData runtimeData = FindRuntimeData(questId);
        return runtimeData?.State ?? EQuestState.NotStarted;
    }

    /// <summary>
    /// 查询任务配置，缺失时输出错误。
    /// </summary>
    private bool TryGetQuestDefinitionOrLog(string questId, out QuestDefinitionSO questDefinition)
    {
        if (TryGetQuestDefinition(questId, out questDefinition))
        {
            return true;
        }

        Debug.LogError($"任务配置不存在：QuestId={questId}");
        return false;
    }

    /// <summary>
    /// 查找任务存档数据。
    /// </summary>
    private QuestRuntimeData FindRuntimeData(string questId)
    {
        return _runtimeDataList?.FirstOrDefault(data => data != null && data.QuestId == questId);
    }

    /// <summary>
    /// 获取任务存档数据；不存在时创建未接取记录。
    /// </summary>
    private QuestRuntimeData GetOrCreateRuntimeData(string questId)
    {
        QuestRuntimeData runtimeData = FindRuntimeData(questId);
        if (runtimeData != null)
        {
            return runtimeData;
        }

        runtimeData = new QuestRuntimeData(questId, EQuestState.NotStarted);
        _runtimeDataList.Add(runtimeData);
        return runtimeData;
    }
}
}
