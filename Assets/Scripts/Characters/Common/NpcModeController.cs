using PlayArk.DialogueSystem.Data;
using PlayArk.StateMachine;
using UnityEngine;

using DreamSeeker.Characters.Player;
using DreamSeeker.Conditions;
using DreamSeeker.Managers;
using DreamSeeker.QuestSystem.Data;

namespace DreamSeeker.Characters
{
    // FriendlyBehaviours 只放：
    //     DialogueNpcTrigger
    //     FriendlyPatrol
    //     其他只在友好状态主动运行的 MonoBehaviour
    // HostileBehaviours 只放：
    //     OldManBrain
    //     OldManActionDriver
    //     Attacker
    //     其他只在敌对状态主动运行的 MonoBehaviour
    //不要放进模式数组：
    //     StateMachineController / OldManController
    //     DamageableHealth
    //     Health
    //     Mover
    //     Animator
    //     SpriteRenderer
    //     Rigidbody2D
    //     Collider2D
    //     配置 SO
    //     纯数据组件
    // 原因：
    //
    // - StateMachineController：由 NpcModeController.StartMachine/StopMachine 单独控制。
    // - DamageableHealth：友好状态也需要接收攻击入口，否则没法判断能不能转敌对。
    // - Health：血量系统不属于模式行为。
    // - Mover：两种模式都可能用，只在切换时 StopMove。
    // - Animator：友好和敌对共用，不应该开关。
    // - Collider2D：对话触发、受击、物理都可能依赖，不能粗暴关。
    /// <summary>
    /// 控制同一个 NPC 在友好模式和敌对模式之间切换。
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcModeController : MonoBehaviour
    {
        [Header("Mode")]
        [Tooltip("当前 NPC 模式")]
        [SerializeField] private NpcMode _currentMode = NpcMode.Friendly;

        [Header("Behaviours")]
        [Tooltip("友好模式启用的能力组件，例如 DialogueNpcTrigger、FriendlyPatrol")]
        [SerializeField] private Behaviour[] _friendlyBehaviours;
        [Tooltip("敌对模式启用的能力组件，例如 Brain、ActionDriver、Attacker")]
        [SerializeField] private Behaviour[] _hostileBehaviours;

        [Header("Hostile Conditions")]
        [Tooltip("允许从友好切到敌对的条件")]
        [SerializeField] private ConditionSO[] _hostileConditions;
        [Tooltip("敌对条件判断关联的任务配置，可为空")]
        [SerializeField] private QuestDefinitionSO _hostileQuestDefinition;

        [Header("Friendly Conditions")]
        [Tooltip("允许从敌对切回友好的条件")]
        [SerializeField] private ConditionSO[] _friendlyConditions;
        [Tooltip("友好条件判断关联的任务配置，可为空")]
        [SerializeField] private QuestDefinitionSO _friendlyQuestDefinition;

        [Header("Components")]
        [Tooltip("切换模式时用于清空残留速度的移动组件")]
        [SerializeField] private Mover _mover;
        [Tooltip("敌对模式使用的状态机控制器")]
        [SerializeField] private StateMachineController _stateMachineController;

        public bool IsFriendly => _currentMode == NpcMode.Friendly;
        public bool IsHostile => _currentMode == NpcMode.Hostile;
        public NpcMode CurrentMode => _currentMode;

        private void Awake()
        {
            // 自动补齐常用组件，允许 Inspector 手动覆盖。
            _mover ??= GetComponent<Mover>();
            _stateMachineController ??= GetComponent<StateMachineController>();
        }

        private void Start()
        {
            // 初始化时按当前模式同步组件开关。
            SetMode(_currentMode);
        }

        /// <summary>
        /// 强制切换 NPC 模式，并同步相关能力组件。
        /// </summary>
        public void SetMode(NpcMode mode)
        {
            _currentMode = mode;

            bool isFriendly = mode == NpcMode.Friendly;
            bool isHostile = mode == NpcMode.Hostile;

            SetBehaviours(_friendlyBehaviours, isFriendly);
            SetBehaviours(_hostileBehaviours, isHostile);

            // 切换大模式时清掉上一套行为留下的速度。
            _mover?.StopMove();

            if (isHostile)
            {
                _stateMachineController?.StartMachine();
            }
            else
            {
                _stateMachineController?.StopMachine();
            }
        }

        /// <summary>
        /// 条件满足时从友好模式切到敌对模式。
        /// </summary>
        public bool TrySwitchToHostile()
        {
            if (IsHostile)
            {
                return true;
            }

            if (!CanSwitchToHostile())
            {
                return false;
            }

            SetMode(NpcMode.Hostile);
            return true;
        }

        /// <summary>
        /// 条件满足时从敌对模式切回友好模式。
        /// </summary>
        public bool TrySwitchToFriendlyAfterDefeat()
        {
            if (IsFriendly)
            {
                return true;
            }

            if (!CanSwitchToFriendly())
            {
                return false;
            }

            SetMode(NpcMode.Friendly);
            return true;
        }

        /// <summary>
        /// 判断当前是否允许进入敌对模式。
        /// </summary>
        private bool CanSwitchToHostile()
        {
            ConditionContext context = new ConditionContext(
                _hostileQuestDefinition,
                gameObject,
                GetPlayerGameObject());

            return ConditionUtility.AreAllMet(_hostileConditions, context);
        }

        /// <summary>
        /// 判断当前是否允许切回友好模式。
        /// </summary>
        private bool CanSwitchToFriendly()
        {
            ConditionContext context = new ConditionContext(
                _friendlyQuestDefinition,
                gameObject,
                GetPlayerGameObject());

            return ConditionUtility.AreAllMet(_friendlyConditions, context);
        }

        /// <summary>
        /// 批量启停指定模式下的能力组件。
        /// </summary>
        private void SetBehaviours(Behaviour[] behaviours, bool enabled)
        {
            if (behaviours == null)
            {
                return;
            }

            foreach (Behaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                behaviour.enabled = enabled;
            }
        }

        /// <summary>
        /// 获取玩家对象，用于条件系统判断触发者。
        /// </summary>
        private GameObject GetPlayerGameObject()
        {
            return GameManager.Instance != null && GameManager.Instance.Player != null
                ? GameManager.Instance.Player.gameObject
                : null;
        }
    }
}
