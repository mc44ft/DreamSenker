namespace PlayArk.DialogueSystem.Events
{
/// <summary>
/// 当前对话文本完成打印事件。
/// </summary>
public readonly struct DialoguePrintCompletedEvent
{
}

/// <summary>
/// 请求推进到下一条对话内容的事件。
/// </summary>
public readonly struct DialogueAdvanceRequestedEvent
{
}

/// <summary>
/// 玩家选择对话选项的事件。
/// </summary>
public readonly struct DialogueChoiceSelectedEvent
{
    /// <summary>
    /// 玩家选择的选项索引。
    /// </summary>
    public int ChoiceIndex { get; }

    /// <summary>
    /// 创建对话选项选择事件。
    /// </summary>
    public DialogueChoiceSelectedEvent(int choiceIndex)
    {
        ChoiceIndex = choiceIndex;
    }
}
}
