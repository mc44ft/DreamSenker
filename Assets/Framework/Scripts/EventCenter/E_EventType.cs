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
    InputUI_PackagePanel,

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
    /// <summary>
    /// 对话节点通知外部打开UI面板事件 --- 参数 DialogueShowPanelEventArgs（面板类型）
    /// </summary>
    Dialogue_ShowPanel,
    /// <summary>
    /// UI面板操作完成事件 --- 参数 Dialogue_PanelFinishedEventArgs
    /// </summary>
    Dialogue_PanelFinished,
    #endregion

    #endregion


    #region CUSTOM EVENT
    #region MAP EVENT
    /// <summary>
    /// 关卡出口触发事件 --- 参数 int（选项的索引）
    /// </summary>
    Map_ExitTrigger,
    #endregion
    #region GAME EVENT
    /// <summary>
    /// Boss死亡事件 --- 参数 GameBossDeadEventArgs（Boss类型）
    /// </summary>
    Game_BossDead,
    /// <summary>
    /// Boss保持死亡事件 --- 参数 GameBossDeadEventArgs（Boss类型）
    /// </summary>
    Game_BossKeepDead,
    /// <summary>
    /// 加成事件 --- 参数 int（恢复数值）
    /// </summary>
    Game_BonusEffect,
    #endregion
    #region PLAYER EVENT
    /// <summary>
    /// 玩家受伤事件 --- 参数 PlayerGetHitEventArgs（玩家健康值信息）
    /// </summary>
    Player_HealthUpdate,
    #endregion
    
    #endregion



}