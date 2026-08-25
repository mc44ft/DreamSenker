namespace DreamSeeker.Framework.Scene
{
/// <summary>
/// 场景异步加载进度变化事件。
/// </summary>
public readonly struct SceneLoadProgressEvent
{
    /// <summary>
    /// 当前场景加载进度，范围为 0 到 1。
    /// </summary>
    public float Progress { get; }

    /// <summary>
    /// 创建场景加载进度事件。
    /// </summary>
    public SceneLoadProgressEvent(float progress)
    {
        Progress = progress;
    }
}

/// <summary>
/// 场景异步加载完成事件。
/// </summary>
public readonly struct SceneLoadCompletedEvent
{
}
}
