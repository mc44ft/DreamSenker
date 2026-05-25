using Cinemachine;
using PlayArk.DialogueSystem.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DreamSeeker.Data;
using DreamSeeker.Data.Configs;
using DreamSeeker.Data.Configs.Character.Player;
using DreamSeeker.Data.Runtime;
using DreamSeeker.CameraSystem;
using DreamSeeker.Inventory;
using DreamSeeker.MapSystem;
using DreamSeeker.MapSystem.SpawnPoints;
using DreamSeeker.QuestSystem;
using DreamSeeker.QuestSystem.Data;
using DreamSeeker.Shared;
using DreamSeeker.UI;

using DreamSeeker.Characters.Player;
using DreamSeeker.Characters.Pet;
using DreamSeeker.MapSystem.Data;

namespace DreamSeeker.Managers
{
[DisallowMultipleComponent]
public class GameManager : SingletonMono<GameManager>
{
    //------------------------ Config ---------------------------------
    [field: SerializeField] public MapDefinitionSO InitialMap { get; private set; }
    [field: SerializeField] public MapConnectionDatabaseSO ConnectionDatabase { get; private set; }
    [field: SerializeField] public GameConfigSO GameConfig { get; private set; }
    [field: SerializeField] public PlayerConfigSO PlayerConfigSO { get; private set; }
    [field: SerializeField] public PackageItemConfigSO PackageItemConfig;
    [field: SerializeField] public QuestListSO QuestList { get; private set; }
    [field: SerializeField] public GameSaveData GameSaveData { get; private set; }
    //------------------------ public Parameter ---------------------------------
    public PlayerController Player { get; private set; }
    //------------------------ Private Parameter ---------------------------------
    private const float DefaultFollowCameraOrthoSize = 5f;//非地图场景默认跟随相机正交视野，开始UI界面是这个值
    private const string CameraConfinerWallName = "Wall";//每张地图中用于限制 Follow 虚拟相机边界的固定物体名
    private CinemachineImpulseSource _impulseSource;//用于处理镜头震动的相机配置
    private PlayerMirrorEffect _playerMirrorEffect;//可选的镜像地图表现组件
    private MapFlowController _mapFlowController;//地图流程控制器，负责地图查询、切图和特殊地图初始化

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
        EventCenter.Instance.AddEventListener<GameBossDeadEventArgs>(EEventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<GameBossDeadEventArgs>(EEventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void OnGameBossDead(object eventSender, GameBossDeadEventArgs args)
    {
        switch (args.BossType)
        {
            case EBossType.Spider:
                GameSaveData.IsKilledSpiderBoss = true;
                CameraManager.Instance?.ExitBossFight();
                TimerManager.Countdown(3, () =>
                {
                    SceneTransition.Instance.ResetLoading(Resources.Load<Sprite>("LoadingMirrorMap1"), 1);
                    TeleportMap(GetMapIdFromEnum(EMapSceneName.MirrorMap1));
                });
                break;
            case EBossType.FoxTwo:
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
        SyncFollowCameraOrthoSize(next.name);
        SyncFollowCameraConfinerBounds();
    }

    /// <summary>
    /// 根据当前场景同步跟随相机正交视野；非地图场景使用默认值
    /// </summary>
    private void SyncFollowCameraOrthoSize(string sceneName)
    {
        float orthoSize = _mapFlowController != null && _mapFlowController.TryGetFollowCameraOrthoSizeBySceneName(sceneName, out float mapOrthoSize)
            ? mapOrthoSize
            : DefaultFollowCameraOrthoSize;

        CameraManager.Instance?.SetFollowCameraOrthoSize(orthoSize);
    }

    /// <summary>
    /// 按固定名称查找当前场景相机边界，并同步到 Follow 虚拟相机 Confiner2D。
    /// </summary>
    private void SyncFollowCameraConfinerBounds()
    {
        GameObject wall = GameObject.Find(CameraConfinerWallName);
        CameraManager.Instance?.SetFollowCameraConfinerBounds(wall);
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

        if (GameSaveData.QuestRuntimeDataList == null)
        {
            GameSaveData.QuestRuntimeDataList = new List<QuestRuntimeData>();
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
        
        Player.PlayerSaveData.PackageData.SyncItemsFromRuntimeDict();
        RunningDataManager.Instance.SaveData(Player.PlayerSaveData);
    }
    /// <summary>
    /// 初始化游戏运行数据、地图流程和玩家实例。玩家点击开始游戏后调用
    /// </summary>
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

        _mapFlowController = new MapFlowController(
            ConnectionDatabase,
            GameSaveData,
            () => Player,
            () => _playerMirrorEffect,
            this);

        MapDefinitionSO saveMap = _mapFlowController.GetRequiredMapById(GameSaveData.SaveMapId);
        GameSaveData.CurrentMapId = saveMap.MapId;

        _mapFlowController.LoadMap(saveMap, () =>
        {
            PlayerController player = InstantiatePlayer(
                PlayerConfigSO.PlayerConfig.PlayerPrefab,
                SpawnPointManager.Instance.GetSpawnPositionFromID(GameSaveData.SavePointID));
            //防止玩家过场景移除
            DontDestroyOnLoad(player.gameObject);
            Player = player;
            PaiMonController.SpawnForPlayer(Player);
            
            CameraManager.Instance?.SetupVCams();
            //镜像表现是特殊地图功能，玩家本体控制器不再持有它
            _playerMirrorEffect = Player.GetComponent<PlayerMirrorEffect>();
            _playerMirrorEffect?.SetMirrorActive(false);

            Player.Initialize(() =>
            {
                //玩家初始化完成后 填充背包管理器数据
                InventoryManager.Instance.SetupData(Player.PlayerSaveData.PackageData, PackageItemConfig);
                //任务系统依赖背包查询，必须在背包数据注入后初始化
                QuestManager.Instance.SetupData(QuestList, GameSaveData.QuestRuntimeDataList);
                _playerMirrorEffect?.SetShadowDarknessStrength(saveMap.PlayerShadowDarknessStrength);

                //加载主面板
                UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton, null, (panel) =>
                {
                    EventCenter.Instance.EventTrigger(
                        EEventType.Player_HealthUpdate,
                        this,
                        new PlayerHealthUpdateEventArgs(Player.DamageableHealth.MaxHealthAmount, Player.DamageableHealth.CurrentHealthAmount));
                    if (!string.IsNullOrEmpty(GameSaveData.CurrentTrackQuestID))
                    {
                        EventCenter.Instance.EventTrigger(
                            EEventType.Quest_TrackChanged, 
                            this, 
                            new StringEventArgs(GameSaveData.CurrentTrackQuestID));
                    }
                    
                });

                //根据当前游戏数据更新地图状态
                _mapFlowController.InitializeCurrentSpecialMap();
            });
        });
    }

    private PlayerController InstantiatePlayer(GameObject playerPrefab, Vector3 position)
    {
        PlayerController player = Instantiate(playerPrefab, position, Quaternion.identity).GetComponent<PlayerController>();
        return player;
    }

    #region Change Scene
    /// <summary>
    /// 普通地图连接换图入口，具体流程交给地图流程控制器。
    /// </summary>
    public void ChangeMap(string pointGuid)
    {
        GetMapFlowControllerOrThrow().ChangeMap(pointGuid);
    }

    /// <summary>
    /// 指定地图传送入口，具体流程交给地图流程控制器。
    /// </summary>
    public void TeleportMap(string mapId, string teleportID = "")
    {
        GetMapFlowControllerOrThrow().TeleportMap(mapId, teleportID);
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
    /// 将地图场景枚举转换为真实场景名。
    /// </summary>
    public string GetSceneNameFromEnum(EMapSceneName sceneName)
    {
        return GetMapFlowControllerOrThrow().GetSceneNameFromEnum(sceneName);
    }

    /// <summary>
    /// 按地图场景枚举获取地图 ID。
    /// </summary>
    public string GetMapIdFromEnum(EMapSceneName sceneName)
    {
        return GetMapFlowControllerOrThrow().GetMapIdFromEnum(sceneName);
    }

    /// <summary>
    /// 尝试获取当前地图日常跟随相机正交视野大小
    /// </summary>
    public bool TryGetCurrentMapFollowCameraOrthoSize(out float orthoSize)
    {
        orthoSize = DefaultFollowCameraOrthoSize;

        if (_mapFlowController == null || !_mapFlowController.TryGetCurrentMapFollowCameraOrthoSize(out float mapOrthoSize))
        {
            return false;
        }

        orthoSize = mapOrthoSize;
        return true;
    }

    /// <summary>
    /// 尝试获取当前地图 Boss 战跟随相机正交视野大小
    /// </summary>
    public bool TryGetCurrentMapBossCameraOrthoSize(out float orthoSize)
    {
        orthoSize = DefaultFollowCameraOrthoSize;

        if (_mapFlowController == null || !_mapFlowController.TryGetCurrentMapBossCameraOrthoSize(out float mapOrthoSize))
        {
            return false;
        }

        orthoSize = mapOrthoSize;
        return true;
    }

    /// <summary>
    /// 尝试获取当前地图对话相机正交视野和Y轴偏移配置
    /// </summary>
    public bool TryGetCurrentMapDialogueCameraSettings(out float orthoSize, out float offsetY)
    {
        orthoSize = DefaultFollowCameraOrthoSize;
        offsetY = 0f;

        if (_mapFlowController == null || !_mapFlowController.TryGetCurrentMapDialogueCameraSettings(out float mapOrthoSize, out float mapOffsetY))
        {
            return false;
        }

        orthoSize = mapOrthoSize;
        offsetY = mapOffsetY;
        return true;
    }

    /// <summary>
    /// 获取地图流程控制器；游戏未初始化时直接抛出明确错误。
    /// </summary>
    private MapFlowController GetMapFlowControllerOrThrow()
    {
        if (_mapFlowController == null)
        {
            throw new InvalidOperationException("MapFlowController 尚未初始化");
        }

        return _mapFlowController;
    }
    #endregion
}
}
