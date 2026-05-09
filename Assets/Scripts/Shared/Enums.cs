
/// <summary>
/// 出生点类型
/// </summary>

namespace DreamSenker.Shared
{
public enum ESpawnType
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
public enum EGameCondition
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
public enum EPackageItemType
{
    RedFruit,
    BlueFruit,
    Chen,
}
public enum EMapSceneName
{
    CampMap,
    MagicMap,
    CaveMap,
    FoxMap,
    MirrorMap1,
    MirrorMap2,
}
public enum EBossType
{
    Spider,
    FoxTwo,
    FoxClone,
    FoxOne,

}
public enum EBonusEffectType
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
}
