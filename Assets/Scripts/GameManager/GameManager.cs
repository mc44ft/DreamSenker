using Cinemachine;
using Data.ScriptableObjects;
using Data.ScriptableObjects.Character.Player;
using PlayArk.DialogueSystem.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GameManager : SingletonMono<GameManager>
{
    //------------------------ Config ---------------------------------
    [field: SerializeField] public MapDefinitionSO InitialMap { get; private set; }
    [field: SerializeField] public MapConnectionDatabaseSO ConnectionDatabase { get; private set; }
    [field: SerializeField] public GameConfigSO GameConfig { get; private set; }
    [field: SerializeField] public PlayerConfigSO PlayerConfigSO { get; private set; }
    [field: SerializeField] public PackageItemConfigSO PackageItemConfig;
    [field: SerializeField] public GameSaveData GameSaveData { get; private set; }
    //------------------------ public Parameter ---------------------------------
    public PlayerController Player { get; private set; }
    //------------------------ Private Parameter ---------------------------------
    private CinemachineImpulseSource _impulseSource;//用于处理镜头震动的相机配置
    private PlayerMirrorEffect _playerMirrorEffect;//可选的镜像地图表现组件

    protected override void Awake()
    {
        base.Awake();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Start()
    {
        UIManager.Instance.ShowPanel<BeginPanel>(E_UILayer.Botton);
    }

    #region Game Event
    private void OnEnable()
    {
        EventCenter.Instance.AddEventListener<DialogueShowPanelEventArgs>(E_EventType.Dialogue_ShowPanel, OnDialogueNodeShowPanel);
        EventCenter.Instance.AddEventListener<GameBossDeadEventArgs>(E_EventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<DialogueShowPanelEventArgs>(E_EventType.Dialogue_ShowPanel, OnDialogueNodeShowPanel);
        EventCenter.Instance.RemoveEventListener<GameBossDeadEventArgs>(E_EventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnDialogueNodeShowPanel(object eventSender, DialogueShowPanelEventArgs args)
    {
        switch (args.UiPanelType)
        {
            case E_DialogueExternalUiPanelType.BounsChoosePanel:
                UIManager.Instance.ShowPanel<BounsChoosePanel>(E_UILayer.Top);
                break;
            case E_DialogueExternalUiPanelType.TaskPublishPanel:
                UIManager.Instance.ShowPanel<TaskPanel>(E_UILayer.Top, (panel) =>
                {
                    panel.Initialize(TaskPanel.E_PanelMode.Publish);
                });
                break;
            case E_DialogueExternalUiPanelType.TaskDeliverPanel:
                UIManager.Instance.ShowPanel<TaskPanel>(E_UILayer.Top, (panel) =>
                {
                    panel.Initialize(TaskPanel.E_PanelMode.Deliver);
                });
                break;
            case E_DialogueExternalUiPanelType.RestoreHealthPanel:
                UIManager.Instance.ShowPanel<RestoreHealthPanel>(E_UILayer.Top);
                break;
            default:
                break;
        }
    }

    private void OnGameBossDead(object eventSender, GameBossDeadEventArgs args)
    {
        switch (args.BossType)
        {
            case E_BossType.Spider:
                GameSaveData.IsKilledSpiderBoss = true;
                TimerManager.Countdown(3, () =>
                {
                    SceneTransition.Instance.ResetLoading(Resources.Load<Sprite>("LoadingMirrorMap1"), 1);
                    TeleportMap(GetMapIdFromEnum(E_MapSceneName.MirrorMap1));
                });
                break;
            case E_BossType.FoxTwo:
                //游戏结束
                GameSaveData.IsKilledFoxBoss = true;
                AudioManager.Instance.StopMusic();

                if (Player != null)
                {
                    Destroy(Player.gameObject);
                    Player = null;
                    _playerMirrorEffect = null;
                }
                //清理出生点
                SpawnPointManager.Instance.ClearSpawnPointDict();
                MapLinkPointManager.Instance.Clear();
                UIManager.Instance.HidePanel<GamePanel>();

                IntroController.Instance.PlayVideo(GameResources.Instance.EndVideoClip, () =>
                {
                    //回到主界面
                    SceneManager.LoadScene("SelectMap");
                    UIManager.Instance.ShowPanel<BeginPanel>(E_UILayer.Botton);
                });
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 场景切换时调用
    /// </summary>
    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        PoolManager.Instance.Clear();
    }
    #endregion
    /// <summary>
    /// 点击继续游戏
    /// </summary>
    public bool LoadGame()
    {
        //加载数据 如果无数据则返回空值
        //加载游戏数据
        bool hasSaveData = RunningDataManager.Instance.LoadData(out GameSaveData gameSaveData);
        //读取游戏数据
        LoadSaveData(gameSaveData);

        //初始化游戏
        InitializeGame();
        return hasSaveData;
    }

    private void LoadSaveData(GameSaveData data)
    {
        if(data != null)
        {
            //读档
            GameSaveData = data;
        }
        else
        {
            //首次加载
            //深拷贝 防止和配置数据产生引用
            GameSaveData = HelpUtilities.DeepCopy(GameConfig.SaveData);
        }

        NormalizeGameSaveData();
    }
    //将数据正常化：未初始化就执行初始化 否则什么都不干
    private void NormalizeGameSaveData()
    {
        if (GameSaveData == null)
        {
            throw new InvalidOperationException("GameSaveData 未配置");
        }

        if (GameSaveData.TriggeredDialogueGuidList == null)
        {
            GameSaveData.TriggeredDialogueGuidList = new List<string>();
        }

        if (string.IsNullOrWhiteSpace(GameSaveData.SaveMapId))
        {
            if (InitialMap == null)
            {
                throw new InvalidOperationException("InitialMap 未配置");
            }

            GameSaveData.SaveMapId = InitialMap.MapId;
        }

        if (string.IsNullOrWhiteSpace(GameSaveData.CurrentMapId))
        {
            GameSaveData.CurrentMapId = GameSaveData.SaveMapId;
        }
    }

    public void SaveDataAll()
    {
        RunningDataManager.Instance.SaveData(GameSaveData);
        RunningDataManager.Instance.SaveData(Player.PlayerSaveData);
    }
    private void InitializeGame()
    {
        if (Player != null)
        {
            Destroy(Player.gameObject);
            Player = null;
            _playerMirrorEffect = null;
        }
        //清理出生点
        SpawnPointManager.Instance.ClearSpawnPointDict();
        MapLinkPointManager.Instance.Clear();
        UIManager.Instance.HidePanel<GamePanel>();

        MapDefinitionSO saveMap = GetRequiredMapById(GameSaveData.SaveMapId);
        GameSaveData.CurrentMapId = saveMap.MapId;

        LoadMap(saveMap, () =>
        {
            PlayerController player = InstantiatePlayer(
                PlayerConfigSO.PlayerConfig.PlayerPrefab,
                SpawnPointManager.Instance.GetSpawnPositionFromID(GameSaveData.SavePointID));
            //防止玩家过场景移除
            DontDestroyOnLoad(player.gameObject);
            Player = player;
            //镜像表现是特殊地图功能，玩家本体控制器不再持有它
            _playerMirrorEffect = Player.GetComponent<PlayerMirrorEffect>();
            _playerMirrorEffect?.SetMirrorActive(false);

            Player.Initialize(() =>
            {
                //玩家初始化完成后 填充背包管理器数据
                InventoryManager.Instance.SetupData(Player.PlayerSaveData.PackageData, PackageItemConfig);
                _playerMirrorEffect?.SetShadowDarknessStrength(saveMap.PlayerShadowDarknessStrength);

                //加载主面板
                UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton, null, (panel) =>
                {
                    EventCenter.Instance.EventTrigger(
                        E_EventType.Player_HealthUpdate,
                        this,
                        new PlayerHealthUpdateEventArgs(Player.Health.MaxHealthAmount, Player.Health.CurrentHealthAmount));
                });

                //根据当前游戏数据更新地图状态
                SpecialMapInitialize();
            });
        });
    }

    private void LoadMap(MapDefinitionSO mapDefinition, Action onFinished = null)
    {
        LoadMapScene(mapDefinition.SceneName, onFinished);
    }

    private void LoadMapScene(string mapName, Action onFinished = null)
    {
        SceneTransition.Instance.LoadScene(mapName, null, onFinished);
    }

    private PlayerController InstantiatePlayer(GameObject playerPrefab, Vector3 position)
    {
        PlayerController player = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<PlayerController>();
        return player;
    }

    private void ChangePlayerPosition(Vector3 position)
    {
        Player.transform.position = position;
    }

    #region Change Scene
    public void ChangeMap(string pointGuid)
    {
        SpawnPointManager.Instance.ClearSpawnPointDict();
        MapLinkPointManager.Instance.Clear();

        MapEndpointData targetEndpoint = GetOtherEndpointOrThrow(GameSaveData.CurrentMapId, pointGuid);
        MapDefinitionSO targetMap = GetRequiredMapById(targetEndpoint.MapId);

        GameSaveData.PreviousMapId = GameSaveData.CurrentMapId;
        GameSaveData.CurrentMapId = targetMap.MapId;

        LoadMapScene(targetMap.SceneName, () =>
        {
            Vector3 position = MapLinkPointManager.Instance.GetPointPositionOrThrow(targetEndpoint.PointGuid);
            ChangePlayerPosition(position);
            _playerMirrorEffect?.SetShadowDarknessStrength(targetMap.PlayerShadowDarknessStrength);
            SpecialMapInitialize();
        });
    }

    public void TeleportMap(string mapId, string teleportID = "")
    {
        SpawnPointManager.Instance.ClearSpawnPointDict();
        MapLinkPointManager.Instance.Clear();
        //即将加载的MapNode
        MapDefinitionSO targetMap = GetRequiredMapById(mapId);
        //更新地图信息
        GameSaveData.PreviousMapId = GameSaveData.CurrentMapId;
        GameSaveData.CurrentMapId = targetMap.MapId;
        //加载新地图
        LoadMapScene(targetMap.SceneName, () =>
        {
            Vector3 position;
            if (!string.IsNullOrWhiteSpace(teleportID))
            {
                position = SpawnPointManager.Instance.GetSpawnPositionFromID(teleportID);
            }
            else
            {
                position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.TeleportPoint);
            }

            ChangePlayerPosition(position);
            _playerMirrorEffect?.SetShadowDarknessStrength(targetMap.PlayerShadowDarknessStrength);
            SpecialMapInitialize();
        });
    }

    //特殊地图的初始化
    private void SpecialMapInitialize()
    {
        if (IsMapScene(GameSaveData.CurrentMapId, E_MapSceneName.CaveMap))
        {
            if (CheckGameCondition(E_GameCondition.SpiderLose))
            {
                EventCenter.Instance.EventTrigger(
                    E_EventType.Game_BossKeepDead,
                    this,
                    new GameBossKeepDeadEventArgs(E_BossType.Spider));

                if (IsMapScene(GameSaveData.PreviousMapId, E_MapSceneName.MirrorMap2))
                {
                    TimerManager.Countdown(4, () =>
                    {
                        AudioManager.Instance.PlayMusic(GameResources.Instance.CommonMapClip);
                    });

                    UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
                    GameSaveData.IsClearMirrorMap = true;
                    Player.SwitchForm();
                    Player.PlayerSaveData.IsUnlockSwitchStateSkill = true;
                    Player.Health.RestoreHealth(Player.Health.MaxHealthAmount);
                }
            }
        }

        if (IsMapScene(GameSaveData.CurrentMapId, E_MapSceneName.MirrorMap1))
        {
            AudioManager.Instance.PlayMusic(GameResources.Instance.MirrorMapClip);
            UIManager.Instance.HidePanel<GamePanel>();
            _playerMirrorEffect?.SetMirrorActive(true);
        }

        if (IsMapScene(GameSaveData.CurrentMapId, E_MapSceneName.MirrorMap2))
        {
            _playerMirrorEffect?.SetMirrorActive(false);
            Player.SwitchForm();
        }
    }
    #endregion

    #region Game Function
    public void DoHitStop(float duration)
    {
        StartCoroutine(HitStopRoutine());

        IEnumerator HitStopRoutine()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }
    }
    /// <summary>
    /// 处理镜头震动
    /// </summary>
    public void CameraShake(float force = 1.0f)
    {
        //这里产生一次震动
        //force参数是一个乘数 如果0.5f 震动力度就是预设的一半
        _impulseSource.GenerateImpulseWithForce(force);
    }
    /// <summary>
    /// 检测游戏条件是否满足
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    public bool CheckGameCondition(E_GameCondition condition)
    {
        switch (condition)
        {
            case E_GameCondition.None:
                return true;
            case E_GameCondition.SpiderWin:
                return GameSaveData.IsMetSpiderBoss && !GameSaveData.IsKilledSpiderBoss;
            case E_GameCondition.SpiderLose:
                return GameSaveData.IsMetSpiderBoss && GameSaveData.IsKilledSpiderBoss;
            case E_GameCondition.FoxWin:
                return GameSaveData.IsMetFoxBoss && !GameSaveData.IsKilledFoxBoss;
            case E_GameCondition.FoundChen:
                return GameSaveData.IsGotChen;
            case E_GameCondition.ClearMirrorMap:
                return GameSaveData.IsClearMirrorMap;
            default:
                return false;
        }
    }

    public string GetSceneNameFromEnum(E_MapSceneName sceneName)
    {
        switch (sceneName)
        {
            case E_MapSceneName.CampMap:
                return "CampMap";
            case E_MapSceneName.MagicMap:
                return "MagicMap";
            case E_MapSceneName.CaveMap:
                return "CaveMap";
            case E_MapSceneName.FoxMap:
                return "FoxMap";
            case E_MapSceneName.MirrorMap1:
                return "MirrorMap1";
            case E_MapSceneName.MirrorMap2:
                return "MirrorMap2";
            default:
                return "CampMap";
        }
    }

    public string GetMapIdFromEnum(E_MapSceneName sceneName)
    {
        return GetRequiredMapBySceneName(GetSceneNameFromEnum(sceneName)).MapId;
    }

    private bool IsMapScene(string mapId, E_MapSceneName sceneName)
    {
        if (string.IsNullOrWhiteSpace(mapId))
        {
            return false;
        }

        return GetRequiredMapById(mapId).SceneName == GetSceneNameFromEnum(sceneName);
    }

    private MapDefinitionSO GetRequiredMapById(string mapId)
    {
        MapDefinitionSO map = FindMapById(mapId);
        if (map == null)
        {
            throw new InvalidOperationException($"未找到 MapDefinitionSO: {mapId}");
        }

        return map;
    }

    private MapDefinitionSO GetRequiredMapBySceneName(string sceneName)
    {
        MapDefinitionSO map = FindMapBySceneName(sceneName);
        if (map == null)
        {
            throw new InvalidOperationException($"未找到场景对应的 MapDefinitionSO: {sceneName}");
        }

        return map;
    }

    private MapDefinitionSO FindMapById(string mapId)
    {
        return GetMapRegistryOrThrow().AllMaps?.FirstOrDefault(map => map != null && map.MapId == mapId);
    }

    private MapDefinitionSO FindMapBySceneName(string sceneName)
    {
        return GetMapRegistryOrThrow().AllMaps?.FirstOrDefault(map => map != null && map.SceneName == sceneName);
    }

    private MapRegistrySO GetMapRegistryOrThrow()
    {
        if (ConnectionDatabase == null)
        {
            throw new InvalidOperationException("ConnectionDatabase 未配置");
        }

        if (ConnectionDatabase.MapRegistry == null)
        {
            throw new InvalidOperationException("ConnectionDatabase.MapRegistry 未配置");
        }

        return ConnectionDatabase.MapRegistry;
    }

    private MapEndpointData GetOtherEndpointOrThrow(string currentMapId, string pointGuid)
    {
        if (ConnectionDatabase == null)
        {
            throw new InvalidOperationException("ConnectionDatabase 未配置");
        }

        //普通连接必须唯一，0 条和多条都不能继续切图
        if (!ConnectionDatabase.TryGetOtherEndpoint(currentMapId, pointGuid, out MapEndpointData targetEndpoint))
        {
            throw new InvalidOperationException(
                $"未找到唯一连接对端，CurrentMapId={currentMapId}, PointGuid={pointGuid}");
        }

        return targetEndpoint;
    }
    #endregion
}
