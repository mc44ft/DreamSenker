namespace DreamSeeker.QuestSystem
{
/// <summary>
/// 当前追踪任务变化事件。
/// </summary>
public readonly struct QuestTrackChangedEvent
{
    /// <summary>
    /// 当前追踪任务的唯一标识。
    /// </summary>
    public string QuestId { get; }

    /// <summary>
    /// 创建任务追踪变化事件。
    /// </summary>
    public QuestTrackChangedEvent(string questId)
    {
        QuestId = questId;
    }
}
}
