namespace DreamSeeker.QuestSystem
{
/// <summary>
/// 任务长期状态，只记录需要存档的稳定结果。
/// </summary>
public enum EQuestState
{
    /// <summary>
    /// 任务尚未接取。
    /// </summary>
    NotStarted,
    /// <summary>
    /// 任务已经接取，正在进行。
    /// </summary>
    Active,
    /// <summary>
    /// 任务已经完成。
    /// </summary>
    Completed,
}
}
