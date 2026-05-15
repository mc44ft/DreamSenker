using System;
using UnityEngine;

namespace DreamSeeker.QuestSystem.Data
{
/// <summary>
/// 任务固定配置，描述任务文本和完成需求。
/// </summary>
[CreateAssetMenu(fileName = "QuestDefinition_", menuName = "ScriptableObject/Quest/Quest Definition")]
public class QuestDefinitionSO : ScriptableObject
{
    [Header("ID")]
    [SerializeField, ReadOnly] private string _questId;

    [Header("TEXT")]
    [SerializeField] private string _title;
    [SerializeField, TextArea] private string _description;
    [Tooltip("任务目标描述 用于TopBar显示")]
    [SerializeField] private string _targetDescription;

    [Header("REQUIREMENTS")]
    [SerializeField] private QuestItemRequirement[] _itemRequirements;

    /// <summary>
    /// 任务稳定 ID，用于存档和运行时查询。
    /// </summary>
    public string QuestId => _questId;
    /// <summary>
    /// 任务标题。
    /// </summary>
    public string Title => _title;
    /// <summary>
    /// 任务描述。
    /// </summary>
    public string Description => _description;
    /// <summary>
    /// 任务目标描述。
    /// </summary>
    public string TargetDescription => _targetDescription;
    /// <summary>
    /// 完成任务需要交付的物品列表。
    /// </summary>
    public QuestItemRequirement[] ItemRequirements => _itemRequirements;

    /// <summary>
    /// 创建或校验配置时，为空 ID 生成一次 GUID。
    /// </summary>
    private void OnValidate()
    {
        EnsureQuestId();
    }

    /// <summary>
    /// ScriptableObject 创建时，为空 ID 生成一次 GUID。
    /// </summary>
    private void OnEnable()
    {
        EnsureQuestId();
    }

    /// <summary>
    /// 确保任务配置拥有稳定 ID。
    /// </summary>
    private void EnsureQuestId()
    {
        if (string.IsNullOrWhiteSpace(_questId))
        {
            _questId = Guid.NewGuid().ToString("N");
        }
    }
}
}
