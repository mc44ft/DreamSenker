/// <summary>
/// 事件类型枚举
/// </summary>
public enum E_EventType
{
    #region SYSTEM EVENT

    #region SCENE EVENT
    /// <summary>
    /// //场景加载进度事件 --- 参数：float（0-1之间的值）
    /// </summary>
    SceneLoadProgress,
    /// <summary>
    /// 场景加载结束事件 --- 参数：无（）
    /// </summary>
    SceneLoadOver,
    #endregion
    #region INPUT UI EVENT
    /// <summary>
    /// 打开设置面板
    /// </summary>
    InputUI_SettingsPanel,
    /// <summary>
    /// 打开背包面板
    /// </summary>
    InputUI_MainMenuPanel,

    #endregion InputAction
    #region INPUT AXIS EVENT
    /// <summary>
    /// 总轴向输入事件 --- 参数：InputEventArgs（）
    /// </summary>
    Input_Axis,
    #endregion
    #region DIALOGUE EVENT
    /// <summary>
    /// 对话框文本显示完成事件 --- 参数：无
    /// </summary>
    Dialogue_PrintShowed,
    /// <summary>
    /// 显示下一个对话框文本事件 --- 参数：无
    /// </summary>
    Dialogue_ContentNext,
    /// <summary>
    /// 选项被点击事件 --- 参数 int（选项的索引）
    /// </summary>
    Dialogue_ChoiceClick,
    #endregion

    #endregion


    #region CUSTOM EVENT
    #region GAME EVENT
    /// <summary>
    /// Boss死亡事件 --- 参数 GameBossDeadEventArgs（Boss类型）
    /// </summary>
    Game_BossDead,
    /// <summary>
    /// Boss保持死亡事件 --- 参数 GameBossDeadEventArgs（Boss类型）
    /// </summary>
    Game_BossKeepDead,
    
    #endregion
    #region PLAYER EVENT
    /// <summary>
    /// 玩家受伤事件 --- 参数 PlayerHealthUpdateEventArgs（玩家健康值信息）
    /// </summary>
    Player_HealthUpdate,
    #endregion
    /// <summary>
    /// 追踪任务变化 --- 参数 string
    /// </summary>
    Quest_TrackChanged, 
    #endregion



}
