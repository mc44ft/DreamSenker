using System;

using DreamSeeker.Characters.Player;
using DreamSeeker.Data.Runtime;
using DreamSeeker.Managers;
using DreamSeeker.MapSystem.Data;
using DreamSeeker.Shared;
using DreamSeeker.UI;

namespace DreamSeeker.MapSystem
{
/// <summary>
/// 特殊地图协调器：只处理进入特殊地图后的表现和存档状态修正。
/// </summary>
public class SpecialMapCoordinator
{
    private readonly GameSaveData _gameSaveData;//当前游戏存档数据
    private readonly Func<PlayerController> _playerGetter;//玩家控制器获取入口
    private readonly Func<PlayerMirrorEffect> _mirrorEffectGetter;//玩家镜像表现组件获取入口
    private readonly object _eventSender;//事件发送者，保持原 GameManager 事件来源

    /// <summary>
    /// 创建特殊地图协调器。
    /// </summary>
    public SpecialMapCoordinator(
        GameSaveData gameSaveData,
        Func<PlayerController> playerGetter,
        Func<PlayerMirrorEffect> mirrorEffectGetter,
        object eventSender)
    {
        _gameSaveData = gameSaveData;
        _playerGetter = playerGetter;
        _mirrorEffectGetter = mirrorEffectGetter;
        _eventSender = eventSender;
    }

    /// <summary>
    /// 根据当前地图和上一张地图，初始化特殊地图表现。
    /// </summary>
    public void InitializeCurrentMap(MapDefinitionSO currentMap, MapDefinitionSO previousMap)
    {
        if (currentMap == null)
        {
            return;
        }

        if (IsMapScene(currentMap, EMapSceneName.CaveMap))
        {
            InitializeCaveMap(previousMap);
        }

        if (IsMapScene(currentMap, EMapSceneName.MirrorMap1))
        {
            InitializeMirrorMap1();
        }

        if (IsMapScene(currentMap, EMapSceneName.MirrorMap2))
        {
            InitializeMirrorMap2();
        }
    }

    /// <summary>
    /// 初始化 CaveMap 的 Boss 死亡保持和梦境通关奖励。
    /// </summary>
    private void InitializeCaveMap(MapDefinitionSO previousMap)
    {
        if (!IsSpiderBossDead())
        {
            return;
        }

        EventCenter.Instance.EventTrigger(
            E_EventType.Game_BossKeepDead,
            _eventSender,
            new GameBossKeepDeadEventArgs(EBossType.Spider));

        if (!IsMapScene(previousMap, EMapSceneName.MirrorMap2))
        {
            return;
        }

        TimerManager.Countdown(4, () =>
        {
            AudioManager.Instance.PlayMusic(GameResources.Instance.CommonMapClip);
        });

        PlayerController player = GetPlayerOrThrow();
        UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
        _gameSaveData.IsClearMirrorMap = true;
        player.SwitchForm();
        player.PlayerSaveData.IsUnlockSwitchStateSkill = true;
        player.DamageableHealth.RestoreHealth(player.DamageableHealth.MaxHealthAmount);
    }

    /// <summary>
    /// 初始化 MirrorMap1 的音乐、UI 和镜像表现。
    /// </summary>
    private void InitializeMirrorMap1()
    {
        AudioManager.Instance.PlayMusic(GameResources.Instance.MirrorMapClip);
        UIManager.Instance.HidePanel<GamePanel>();
        _mirrorEffectGetter?.Invoke()?.SetMirrorActive(true);
    }

    /// <summary>
    /// 初始化 MirrorMap2 的镜像关闭和玩家形态切换。
    /// </summary>
    private void InitializeMirrorMap2()
    {
        _mirrorEffectGetter?.Invoke()?.SetMirrorActive(false);
        GetPlayerOrThrow().SwitchForm();
    }

    /// <summary>
    /// 判断当前蜘蛛 Boss 是否处于已击杀状态。
    /// </summary>
    private bool IsSpiderBossDead()
    {
        return _gameSaveData != null && _gameSaveData.IsMetSpiderBoss && _gameSaveData.IsKilledSpiderBoss;
    }

    /// <summary>
    /// 判断地图配置是否对应指定场景枚举。
    /// </summary>
    private bool IsMapScene(MapDefinitionSO map, EMapSceneName sceneName)
    {
        return map != null && map.SceneName == MapRuntimeQuery.GetSceneNameFromEnum(sceneName);
    }

    /// <summary>
    /// 获取玩家控制器；特殊地图逻辑需要玩家时，缺失直接暴露配置错误。
    /// </summary>
    private PlayerController GetPlayerOrThrow()
    {
        PlayerController player = _playerGetter?.Invoke();
        if (player == null)
        {
            throw new InvalidOperationException("特殊地图初始化需要 PlayerController，但当前玩家为空");
        }

        return player;
    }
}
}
