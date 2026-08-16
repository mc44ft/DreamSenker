namespace DreamSeeker.Characters.Player
{
/// <summary>
/// 玩家生命值变化事件。
/// </summary>
public readonly struct PlayerHealthChangedEvent
{
    /// <summary>
    /// 玩家最大生命值。
    /// </summary>
    public int MaxHealthAmount { get; }

    /// <summary>
    /// 玩家当前生命值。
    /// </summary>
    public int CurrentHealthAmount { get; }

    /// <summary>
    /// 创建玩家生命值变化事件。
    /// </summary>
    public PlayerHealthChangedEvent(int maxHealthAmount, int currentHealthAmount)
    {
        MaxHealthAmount = maxHealthAmount;
        CurrentHealthAmount = currentHealthAmount;
    }
}
}
