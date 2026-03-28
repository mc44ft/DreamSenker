
public enum E_LevelExitDirection
{
    East,
    South,
    West,
    North
}
public enum E_AiState
{
    None = 0,
    Idle,
    GetHit,
    Patrol,
    Chase,
    Attack,
    Death,
}
/// <summary>
/// 出生点类型
/// </summary>
public enum E_SpawnType
{
    FromSavePoint,//读档复活 或 新游戏开始
    EastPoint,
    EastHiddenPoint,
    WestPoint,
    WestHiddenPoint,
    SouthPoint,
    SouthHiddenPoint,
    NorthPoint,
    NorthHiddenPoint,
    /// <summary>
    /// 特殊的传送出生点
    /// </summary>
    TeleportPoint,
}
/// <summary>
/// 出口类型
/// </summary>
public enum E_ExitType
{
    East,
    EastHidden,
    West,
    WestHidden,
    South,
    SouthHidden,
    North,
    NorthHidden,
}
public enum E_GameCondition
{
    /// <summary>
    /// 不判断任何条件
    /// </summary>
    None,
    /// <summary>
    /// 蜘蛛赢
    /// </summary>
    SpiderWin,
    /// <summary>
    /// 蜘蛛输
    /// </summary>
    SpiderLose,
    /// <summary>
    /// 狐狸赢
    /// </summary>
    FoxWin,
    /// <summary>
    /// 找到晨露梦核
    /// </summary>
    FoundChen,
    /// <summary>
    /// 通关梦境地图
    /// </summary>
    ClearMirrorMap,
}
public enum E_PackageItemID
{
    RedFruit,
    BlueFruit,
    Chen,
}
public enum E_MapSceneName
{
    CampMap,
    MagicMap,
    CaveMap,
    FoxMap,
    MirrorMap1,
    MirrorMap2,
}
public enum E_BossType
{
    Spider,
    FoxTwo,
    FoxClone,
    FoxOne,

}
public enum E_BonusEffectType
{
    None,
    /// <summary>
    /// 回血
    /// </summary>
    RestoreHealth,
    /// <summary>
    /// 回蓝
    /// </summary>
    RestoreMana,
}

/// <summary>
/// 测试枚举 - 用于演示 Claude Code 功能
/// </summary>
public enum E_TestEnum
{
    TestValue1,
    TestValue2,
    TestValue3,
}