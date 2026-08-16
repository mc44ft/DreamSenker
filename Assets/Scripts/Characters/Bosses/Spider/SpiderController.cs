using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DreamSeeker.CameraSystem;
using DreamSeeker.Combat.Health;
using DreamSeeker.Data.Configs.Character.Monster;
using DreamSeeker.Framework.Events;
using DreamSeeker.Shared;

namespace DreamSeeker.Characters.Bosses
{
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Health))]
public class SpiderController : MonoBehaviour, IDamageable, ITouchDamageable
{
    [field: SerializeField] public List<Waypoint> Waypoints { get; private set; }
    [field: SerializeField] public SpiderConfigSO SpiderConfig { get; private set; }
    [field: SerializeField] public GameObject EggGameObject { get; private set; }
    [field: SerializeField] public BoxCollider2D SkillVenomBiteZone {  get; private set; }
    [field: SerializeField] public SkillShakeHit RedWebMask {  get; private set; }
    public Waypoint WebCenterWaypoint;//蜘蛛丝中心航点

    public Transform Player {  get; private set; }

    //------------------------------- Component ------------------------------------
    public Rigidbody2D Rigidbody {  get; private set; }
    public SpriteRenderer SpriteRenderer {  get; private set; }
    public Animator Animator { get; private set; }
    public Health Health { get; private set; }

    //------------------------------- Private Parameter ------------------------------------
    private MachineManager<SpiderController> _machineManager;
    private Coroutine m_gethitCoroutine = null;
    public int FaceLeft = 1;
    //------------------------------- Public Parameter ------------------------------------
    public Waypoint CurrentWaypoint { get; set; }//当前所在航点
    public Waypoint TargetWaypoint { get; set; }//本次目标航点
    public List<Waypoint> NowPath { get; set; }//本次航线
    public int HealthAmount {  get; set; }
    public float MoveSpeed { get; set; }//实际使用的移动速度 切换速度值时改变此速度
    public float CooldownTimer { get; set; }//一个计时器 用于各状态拖延时间
    public float SkillHealCooldownTimer { get; set; }
    public float SkillVenomBiteCooldownTimer { get; set; }
    public float SkillWebCooldownTimer { get; set; }
    public float SkillhakeCooldownTimer { get; set; }
    //------------------------------- State Instance ------------------------------------
    public SpiderIdleState SpiderIdleState { get; private set; }
    public SpiderCrawlState SpiderCrawlState {  get; private set; }
    public SpiderDeathState SpiderDeathState { get; private set; }
    public SpiderSkillHealState SpiderSkillHealState { get; private set; }
    public SpiderSkillVenomBiteState SpiderSkillVenomBiteState { get; private set; }
    public SpiderSkillWebState SpiderSkillWebState { get; private set; }
    public SpiderSkillShakeState SpiderSkillShakeState { get; private set; }
    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody2D>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        Health = GetComponent<Health>();

        _machineManager = new MachineManager<SpiderController>();

        //实例所有状态
        SpiderIdleState = new SpiderIdleState(this, _machineManager);
        SpiderCrawlState = new SpiderCrawlState(this, _machineManager);
        SpiderDeathState = new SpiderDeathState(this, _machineManager);
        SpiderSkillHealState = new SpiderSkillHealState(this, _machineManager);
        SpiderSkillVenomBiteState = new SpiderSkillVenomBiteState(this, _machineManager);
        SpiderSkillWebState = new SpiderSkillWebState(this,_machineManager);
        SpiderSkillShakeState = new SpiderSkillShakeState(this, _machineManager);

    }
    private void Start()
    {
        //初始化当前航点位置
        CurrentWaypoint = GetNearestWaypoint(transform.position);

        //初始化技能冷却计时器
        SkillHealCooldownTimer = SpiderConfig.SkillHealCooldown;
        SkillVenomBiteCooldownTimer = SpiderConfig.SkillVenomBiteCooldown;
        SkillWebCooldownTimer = SpiderConfig.SkillWebCooldown;
        SkillhakeCooldownTimer = SpiderConfig.SkillShakeCooldown;

        //初始化健康值组件
        Health.Initialize(SpiderConfig.MaxHealthAmount, SpiderConfig.MaxHealthAmount);

        //保证一开始Egg不显示
        EggGameObject.SetActive(false);
    }
    private void Update()
    {
        //更新各技能计时器
        UpdateTimer();
        _machineManager.LogicUpdate();
    }
    
    private IEnumerator GetHitRoutine(float time = 0.2f)//受击并非一个状态
    {
        SpriteRenderer.color = Color.red;
        yield return new WaitForSeconds(time);
        SpriteRenderer.color = Color.white;
    }
    private void FixedUpdate()
    {
        _machineManager.PhysicsUpdate();
    }

    
    public void UpdateFace(int isFaceLeft)
    {
        FaceLeft = isFaceLeft;

        transform.localScale = new Vector3(FaceLeft, 1, 1);
    }
    private void UpdateTimer()
    {
        SkillHealCooldownTimer -= Time.deltaTime;
        SkillVenomBiteCooldownTimer -= Time.deltaTime;
        SkillWebCooldownTimer -= Time.deltaTime;
        SkillhakeCooldownTimer -= Time.deltaTime;
    }
    public void KeepDeathState()
    {
        //赐死
        Health.ApplyDamage(99999);
        _machineManager.TransitionTo(SpiderDeathState);
    }
    public void BindingPlayer(Transform player)
    {
        if (Health.IsDead)
            return;
        Player = player;

        //切换到Idle状态 开始作战
        _machineManager.Initialize(SpiderIdleState);
    }
    /// <summary>
    /// 获取离传入位置最近的航点
    /// </summary>
    /// <returns></returns>
    public Waypoint GetNearestWaypoint(Vector3 position)
    {
        float distance = float.MaxValue;
        Waypoint resultWaypoint = null;
        foreach (var waypoint in Waypoints)
        {
            float newDistance = Vector3.Distance(position, waypoint.transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                resultWaypoint = waypoint;
            }
        }
        return resultWaypoint;
    }
    public List<Waypoint> FindPathAstar(Waypoint currentWaypoint, Waypoint targetWaypoint)
    {
        //closedSet是存储所有已经访问的节点 保证下次不会再将其加入openSet中

        Dictionary<Waypoint, WaypointInfo> waypointInfoDict = new Dictionary<Waypoint, WaypointInfo>();//存储所有航点信息的字典 充当closedSet
        List<Waypoint> openSet = new List<Waypoint>();//当前待前进的航点

        //添加开始节点到 openSet
        openSet.Add(currentWaypoint);
        //设置开始节点的初始信息
        waypointInfoDict[currentWaypoint] = new WaypointInfo() { FromWaypoint = null, G_Cost = 0, H_Cost = 0 };

        while (openSet.Count > 0)
        {
            //先对 openSet进行排序
            openSet.Sort((a, b) =>
            {
                return waypointInfoDict[a].F_Cost > waypointInfoDict[b].F_Cost ? 1 : -1;
            });

            //从容器中取出最低成本的航点
            Waypoint waypoint = openSet[0];
            openSet.RemoveAt(0);

            //找到终点 结束循环
            if (waypoint == targetWaypoint)
            {
                List<Waypoint> path = new List<Waypoint>();
                path.Add(waypoint);

                Waypoint key = waypoint;
                while (waypointInfoDict[key].FromWaypoint != null)
                {
                    //添加到路径中
                    path.Add(waypointInfoDict[key].FromWaypoint);

                    //更新键值
                    key = waypointInfoDict[key].FromWaypoint;
                }
                return path;
            }

            //遍历当前航点的邻居航点
            foreach (var connectedNode in waypoint.ConnectedNodes)
            {
                //计算新的移动代价：上一个航点的移动代价 + 当前航点与上一个航点的移动代价
                float g_Cost = waypointInfoDict[waypoint].G_Cost + Vector3.Distance(connectedNode.transform.position, waypoint.transform.position);

                //查找这个邻居航点是否存在于 closedSet中
                bool isVisited = waypointInfoDict.ContainsKey(connectedNode);
                if (!isVisited)//如果不存在 初始化其数据 并添加到 openSet中
                {
                    //添加到openSet
                    openSet.Add(connectedNode);

                    //更新航点信息
                    waypointInfoDict[connectedNode] = new WaypointInfo()
                    {
                        FromWaypoint = waypoint,
                        G_Cost = g_Cost,
                        H_Cost = Vector3.Distance(connectedNode.transform.position, targetWaypoint.transform.position)
                    };
                }
                else if (g_Cost < waypointInfoDict[connectedNode].G_Cost)//存在但是新计算的g_Cost比之前的小，需要更新值
                {
                    //更新航点信息
                    waypointInfoDict[connectedNode] = new WaypointInfo()
                    {
                        FromWaypoint = waypoint,
                        G_Cost = g_Cost,
                        H_Cost = Vector3.Distance(connectedNode.transform.position, targetWaypoint.transform.position)
                    };
                }
            }
        }
        return null;

    }
    public int GetTouchDamage()
    {
        return SpiderConfig.TouchDamage;
    }

    public void TakeDamage(int damage, Vector2 attackDirection)
    {
        if (Health.IsDead) return;
        Health.ApplyDamage(damage);

        if(m_gethitCoroutine != null)
        {
            StopCoroutine(m_gethitCoroutine);
        }
        m_gethitCoroutine = StartCoroutine(GetHitRoutine());

        if (Health.IsDead)
        {
            _machineManager.TransitionTo(SpiderDeathState);

            //通知外界该Boss已死
            EventBus.Publish(new GameBossDiedEvent(EBossType.Spider, gameObject));
        }
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void Select()
    {
        //蜘蛛在玩家获取2形态前不会与玩家发生战斗
    }

    public void Deselect()
    {
        
    }

    private struct WaypointInfo
    {
        public Waypoint FromWaypoint;//上一个节点
        public float G_Cost;
        public float H_Cost;
        public float F_Cost => G_Cost + H_Cost;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(SkillVenomBiteZone.transform.position, SkillVenomBiteZone.bounds.size);
    }
#endif
}
}
