using Cinemachine;
using DialogueSystem.Misc;
using MapSystem.Graph;
using MapSystem.Nodes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
[DisallowMultipleComponent]
public class GameManager : SingletonMono<GameManager>
{
    //------------------------ Config ---------------------------------
    [field: SerializeField] public MapGraph MapGraph { get; private set; }
    [field: SerializeField] public GameConfigSO GameConfig { get; private set; }
    [field: SerializeField] public PlayerConfigSO PlayerConfig { get; private set; }
    [field: SerializeField] public PackageItemConfigSO PackageItemConfig;
    [field: SerializeField] public GameSaveData GameSaveData { get; private set; }
    //------------------------ public Parameter ---------------------------------
    public PlayerController Player { get; private set; }
    //------------------------ Private Parameter ---------------------------------
    private CinemachineImpulseSource _impulseSource;//用于处理镜头震动的相机配置
    protected override void Awake()
    {
        base.Awake();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    private void Start()
    {
        //加载Begin UI面板
        UIManager.Instance.ShowPanel<BeginPanel>(E_UILayer.Botton);
    }
    #region Game Event
    private void OnEnable()
    {
        //监听玩家离开地图事件
        EventCenter.Instance.AddEventListener<MapExitTriggerEventArgs>(E_EventType.Map_ExitTrigger, OnMapExitTrigger);
        //监听DialogueNodeExternalUI节点的显示面板事件
        EventCenter.Instance.AddEventListener<DialogueShowPanelEventArgs>(E_EventType.Dialogue_ShowPanel, OnDialogueNodeShowPanel);
        //关心Boss死亡事件
        EventCenter.Instance.AddEventListener<GameBossDeadEventArgs>(E_EventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    private void OnDisable()
    {
        EventCenter.Instance.RemoveEventListener<MapExitTriggerEventArgs>(E_EventType.Map_ExitTrigger, OnMapExitTrigger);
        EventCenter.Instance.RemoveEventListener<DialogueShowPanelEventArgs>(E_EventType.Dialogue_ShowPanel, OnDialogueNodeShowPanel);
        EventCenter.Instance.RemoveEventListener<GameBossDeadEventArgs>(E_EventType.Game_BossDead, OnGameBossDead);
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    private void OnMapExitTrigger(object eventSender, MapExitTriggerEventArgs args)
    {
        ChangeMap(args.ExitType);
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
                    TeleportMap(GetSceneNameFromEnum(E_MapSceneName.MirrorMap1));
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
                }
                //清理出生点
                SpawnPointManager.Instance.ClearSpawnPointDict();
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
        }
        //清理出生点
        SpawnPointManager.Instance.ClearSpawnPointDict();
        UIManager.Instance.HidePanel<GamePanel>();


        //加载地图
        //如果是读档 PreviousMapNode_guid是有值的 如果第一次进入游戏 PreviousMapNode_guid没有值
        LoadMap(GetSceneNameFromEnum(GameSaveData.SaveMapSceneName), () =>
        {
            //场景加载完毕后
            //实例化玩家
            //场景加载完毕 此时出生点已经注册完毕
            PlayerController player = InstantiatePlayer(PlayerConfig.PlayerPrefab,
                SpawnPointManager.Instance.GetSpawnPositionFromID(GameSaveData.SavePointID));
            //防止玩家过场景移除
            DontDestroyOnLoad(player.gameObject);
            Player = player;

            Player.Initialize(() =>
            {
                //玩家初始化完成后 填充背包管理器数据
                InventoryManager.Instance.SetupData(Player.PlayerSaveData.PackageData, PackageItemConfig);
                //初始化玩家的阴影数据
                Player.SetShadowDarknessStrength(MapGraph.GetMapNodeFromGuid(GameSaveData.CurrentMapNodeGuid).MapData.PlayerShadowDarknessStrength);

                //加载主面板
                UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton, null, (panel) =>
                {
                    //刷新玩家血量
                    EventCenter.Instance.EventTrigger(E_EventType.Player_HealthUpdate, this,
                        new PlayerHealthUpdateEventArgs(Player.Health.MaxHealthAmount, Player.Health.CurrentHealthAmount));
                });

                //根据当前游戏数据更新地图状态
                UpdateMapFromGameSaveData();
                Debug.Log(Application.persistentDataPath);
            });
        });
        
    }
    /// <summary>
    /// 加载存档的Map
    /// </summary>
    private void LoadMap(string mapName, Action onFinished = null)
    {
        MapNode node = MapGraph.Nodes.Where(node => node.MapData.MapSceneName == mapName).FirstOrDefault();
        GameSaveData.CurrentMapNodeGuid = node.guid;

        

        LoadMapScene(mapName, onFinished);
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
    /// <summary>
    /// 地图切换
    /// </summary>
    private void ChangeMap(E_ExitType exitType)
    {
        //清空SpawnPointDict
        SpawnPointManager.Instance.ClearSpawnPointDict();

        //即将加载的MapNode
        MapNode mapNode = MapGraph.GetMapNodeFromGuid(GameSaveData.CurrentMapNodeGuid)
            .GetConnectionNodeFromOutputPort(exitType);
        //更新地图信息
        GameSaveData.PreviousMapNodeGuid = GameSaveData.CurrentMapNodeGuid;
        GameSaveData.CurrentMapNodeGuid = mapNode.guid;

        //加载新地图
        LoadMapScene(mapNode.MapData.MapSceneName, () =>
        {
            //找到对应的出生点位
            Vector3 position = Vector3.one;
            switch (exitType)
            {
                case E_ExitType.East:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.WestPoint);
                    break;
                case E_ExitType.EastHidden:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.WestHiddenPoint);
                    break;
                case E_ExitType.West:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.EastPoint);
                    break;
                case E_ExitType.WestHidden:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.EastHiddenPoint);
                    break;
                case E_ExitType.South:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.NorthPoint);
                    break;
                case E_ExitType.SouthHidden:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.NorthHiddenPoint);
                    break;
                case E_ExitType.North:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.SouthPoint);
                    break;
                case E_ExitType.NorthHidden:
                    position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.SouthHiddenPoint);
                    break;
            }
            //切换玩家位置
            ChangePlayerPosition(position);

            //更新阴影效果
            Player.SetShadowDarknessStrength(mapNode.MapData.PlayerShadowDarknessStrength);

            //根据当前游戏数据更新地图状态
            UpdateMapFromGameSaveData();
        });
    }
    /// <summary>
    /// 地图传送
    /// </summary>
    public void TeleportMap(string mapSceneName, string teleportID = "")
    {
        //清空SpawnPointDict
        SpawnPointManager.Instance.ClearSpawnPointDict();

        //即将加载的MapNode
        MapNode mapNode = MapGraph.Nodes.Where(node => node.MapData.MapSceneName == mapSceneName).FirstOrDefault();

        //更新地图信息
        GameSaveData.PreviousMapNodeGuid = GameSaveData.CurrentMapNodeGuid;
        GameSaveData.CurrentMapNodeGuid = mapNode.guid;

        //加载新地图
        LoadMapScene(mapNode.MapData.MapSceneName, () =>
        {
            //找到传送出生点位
            Vector3 position;
            if (teleportID != "")
            {
                position = SpawnPointManager.Instance.GetSpawnPositionFromID(teleportID);
            }
            else
            {
                position = SpawnPointManager.Instance.GetSpawnPositionFromSpawnType(E_SpawnType.TeleportPoint);
            }
            //切换玩家位置
            ChangePlayerPosition(position);

            //更新阴影效果
            Player.SetShadowDarknessStrength(mapNode.MapData.PlayerShadowDarknessStrength);

            //根据当前游戏数据更新地图状态
            UpdateMapFromGameSaveData();
        });
    }
    private void UpdateMapFromGameSaveData()
    {

        string mapSceneName = MapGraph.GetMapNodeFromGuid(GameSaveData.CurrentMapNodeGuid).MapData.MapSceneName;
        //山洞地图处理
        if (mapSceneName == GetSceneNameFromEnum(E_MapSceneName.CaveMap))
        {
            if (CheckGameCondition(E_GameCondition.SpiderLose))
            {
                //让蜘蛛恢复死亡状态
                EventCenter.Instance.EventTrigger(E_EventType.Game_BossKeepDead, this, new GameBossKeepDeadEventArgs(E_BossType.Spider));

                //从梦境地图回来时触发
                if (MapGraph.GetMapNodeFromGuid(GameSaveData.PreviousMapNodeGuid).MapData.MapSceneName == "MirrorMap2")
                {
                    TimerManager.Countdown(4, () =>
                    {
                        AudioManager.Instance.PlayMusic(GameResources.Instance.CommonMapClip);
                    });

                    UIManager.Instance.ShowPanel<GamePanel>(E_UILayer.Botton);
                    GameSaveData.IsClearMirrorMap = true;
                    //玩家切换回一形态
                    Player.SwitchForm();
                    //玩家解锁切换形态的能力
                    Player.PlayerSaveData.IsUnlockSwitchStateSkill = true;

                    //回满血
                    Player.Health.RestoreHealth(Player.Health.MaxHealthAmount);
                    EventCenter.Instance.EventTrigger(E_EventType.Player_HealthUpdate, this,
                        new PlayerHealthUpdateEventArgs(Player.Health.MaxHealthAmount, Player.Health.CurrentHealthAmount));
                }
            }
        }
        //梦境地图1处理
        if (mapSceneName == GetSceneNameFromEnum(E_MapSceneName.MirrorMap1))
        {
            AudioManager.Instance.PlayMusic(GameResources.Instance.MirrorMapClip);
            UIManager.Instance.HidePanel<GamePanel>();
            Player.SetMirrorActive(true);
        }
        //梦境地图2处理
        if (mapSceneName == GetSceneNameFromEnum(E_MapSceneName.MirrorMap2))
        {
            Player.SetMirrorActive(false);
            Player.SwitchForm();
            
        }
    }



    #endregion
    #region Game Function
    /// <summary>
    /// ScaleTime暂停指定时间
    /// 处理玩家受击顿帧
    /// </summary>
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
    #endregion

    
}
