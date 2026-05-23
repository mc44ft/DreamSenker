# NPC友好敌对模式切换重构方案
## 设计说明
同一个 NPC 既可能是可对话角色，也可能在条件变化后变成可攻击敌人。
举例：
OldMan是一个在森林中乱转的疯老头，在玩家没有接到《击杀疯老头》的任务前，疯老头仅可对话，不会受到玩家攻击，
在玩家接受《击杀疯老头》任务后，疯老头依然保持可对话行为，但是可以被玩家攻击，玩家攻击后 转换为 Hostile状态
## 计划说明

本计划要解决的问题是：同一个 NPC 既可能是可对话角色，也可能在条件变化后变成可攻击敌人。当前项目里这两类对象分得比较开：可对话 NPC 主要依赖 `DialogueNpcTrigger`，可击杀敌人主要依赖 `StateMachineController`、`OldManBrain`、`OldManActionDriver`、`Attacker`、`DamageableHealth`。

本计划采用同实体模式切换方案：不切换预制体，也不把对话逻辑塞进战斗状态机，而是在同一个 NPC 实体上增加模式控制器，由模式决定当前开启哪些能力。

执行后，一个 NPC 可以在友好状态下巡逻、停下（小范围检测到玩家后）、对话，也可以切换到敌对状态后追击、攻击、受击、死亡。特殊敌人可以通过健康系统限制最低血量，避免进入死亡流程，然后按条件切回友好状态并重新允许对话。
只需要对话、永远不会进入战斗的 NPC 不需要挂 `NpcModeController`，继续只使用 `DialogueNpcTrigger`。

## 结论

本计划要做的是“同一个实体的模式切换”。

- 新增 NPC 模式枚举。
- 新增 NPC 模式控制组件。
- 新增友好巡逻组件。
- 修改受击入口，让友好 NPC 被攻击时可以切换敌对。
- 修改健康系统，让特殊敌人的血量可以被限制在最低值以上，避免进入死亡流程。
- 保持 `DialogueNpcTrigger` 只负责对话。
- `DialogueNpcTrigger` 兼容纯对话 NPC，没有 `NpcModeController` 时仍按原逻辑工作。
- 保持 `StateMachineController` 只负责敌对行动和战斗状态。
- `StateMachineController` 兼容纯战斗NPC和玩家，没有 `NpcModeController` 时仍按原逻辑工作。

## 最终结构

```text
NpcModeController
  -> 控制当前模式
  -> 开关 DialogueNpcTrigger
  -> 开关 FriendlyPatrol
  -> 调用 StateMachineController.StartMachine / StopMachine
  -> 开关 OldManBrain / OldManActionDriver / Attacker
  -> 判断 Hostile 是否允许切回 Friendly

Friendly
  -> DialogueNpcTrigger
  -> FriendlyPatrol
  -> Mover

Hostile
  -> StateMachineController
  -> OldManBrain
  -> OldManActionDriver
  -> Attacker
  -> DamageableHealth
  -> 最低血量限制

纯对话 NPC
  -> DialogueNpcTrigger
  -> SpriteRenderer
  -> Trigger Collider

纯战斗角色
  -> StateMachineController
  -> Brain / ActionDriver / Attacker
  -> DamageableHealth
```

核心类职责伪代码：

```csharp
public class NpcModeController : MonoBehaviour
{
    // 当前 NPC 模式
    [SerializeField] private NpcMode _currentMode = NpcMode.Friendly;

    // 友好模式启用的组件，例如 DialogueNpcTrigger、FriendlyPatrol
    [SerializeField] private Behaviour[] _friendlyBehaviours;

    // 敌对模式启用的组件，例如 OldManBrain、OldManActionDriver、Attacker
    // 不要把 StateMachineController 放进这个数组，状态机由 _stateMachineController 单独控制
    [SerializeField] private Behaviour[] _hostileBehaviours;

    // 允许从友好切换为敌对的通用条件
    [SerializeField] private ConditionSO[] _hostileConditions;

    // 敌对条件判断需要的任务配置，可为空
    [SerializeField] private QuestDefinitionSO _hostileQuestDefinition;

    // 允许从敌对切回友好的通用条件
    [SerializeField] private ConditionSO[] _friendlyConditions;

    // 友好条件判断需要的任务配置，可为空
    [SerializeField] private QuestDefinitionSO _friendlyQuestDefinition;

    // 移动能力组件，用于切换模式时停止上一种模式留下的速度
    [SerializeField] private Mover _mover;

    // 状态机控制器
    [SerializeField] private StateMachineController _stateMachineController;

    // 当前是否是友好状态
    public bool IsFriendly => _currentMode == NpcMode.Friendly;

    // 当前是否是敌对状态
    public bool IsHostile => _currentMode == NpcMode.Hostile;

    // 强制切换 NPC 模式
    public void SetMode(NpcMode mode)
    {
        _currentMode = mode;

        bool isFriendly = mode == NpcMode.Friendly;
        bool isHostile = mode == NpcMode.Hostile;

        SetBehaviours(_friendlyBehaviours, isFriendly);
        SetBehaviours(_hostileBehaviours, isHostile);

        _mover?.StopMove();

        if (isHostile)
            _stateMachineController?.StartMachine();
        else
            _stateMachineController?.StopMachine();
    }

    // 尝试切换到敌对模式
    public bool TrySwitchToHostile()
    {
        if (IsHostile)
            return true;

        if (!CanSwitchToHostile())
            return false;

        SetMode(NpcMode.Hostile);
        return true;
    }

    // 判断当前是否允许从友好切换到敌对
    private bool CanSwitchToHostile()
    {
        ConditionContext context = new ConditionContext
        (
            _hostileQuestDefinition,
            gameObject,
            GameManager.Instance.Player.gameObject
        );

        return ConditionUtility.AreAllMet(_hostileConditions, context);
    }

    // 尝试切回友好模式
    public bool TrySwitchToFriendlyAfterDefeat()
    {
        if (IsFriendly)
            return true;

        if (!CanSwitchToFriendly())
            return false;

        SetMode(NpcMode.Friendly);
        return true;
    }

    // 判断当前是否允许从敌对切回友好
    private bool CanSwitchToFriendly()
    {
        ConditionContext context = new ConditionContext
        (
            _friendlyQuestDefinition,
            gameObject,
            GameManager.Instance.Player.gameObject
        );

        return ConditionUtility.AreAllMet(_friendlyConditions, context);
    }

    // 批量启停能力组件
    private void SetBehaviours(Behaviour[] behaviours, bool enabled)
    {
        foreach (var behaviour in behaviours)
        {
            if (behaviour == null)
                continue;

            behaviour.enabled = enabled;
        }
    }
}
```

Inspector 配置示例：

```text
OldMan
  FriendlyBehaviours
    -> DialogueNpcTrigger
    -> FriendlyPatrol

  HostileBehaviours
    -> OldManBrain
    -> OldManActionDriver
    -> Attacker
    -> 不放 OldManController / StateMachineController

  HostileConditions
    -> QuestStateConditionSO 或 QuestCompletableConditionSO

  FriendlyConditions
    -> QuestStateConditionSO 或 AlwaysTrueConditionSO

  LimitMinHealth
    -> true

  MinHealth
    -> 1

  Mover
    -> 当前对象上的 Mover

  StateMachineController
    -> 当前对象上的 OldManController
```

`TrySwitchToHostile()` 失败时的行为：

```text
条件不满足
  -> 不切换 Hostile
  -> 不扣血
  -> 不触发受击硬直
  -> 不触发 TakeDamage 谓词
  -> 不启动状态机

条件满足
  -> 切换 Hostile
  -> 第一次攻击正常扣血
  -> 正常触发受击硬直
  -> 正常触发 TakeDamage 谓词
```

`TrySwitchToFriendlyAfterDefeat()` 的行为：

```text
当前是 Hostile
  -> 调用方确认对象未死亡
  -> 判断 FriendlyConditions
  -> 条件满足时切回 Friendly
  -> 条件不满足时保持 Hostile
```

`DamageableHealth` 的最低血量行为：

```text
未启用最低血量限制
  -> 血量可以降到 0
  -> 正常进入死亡流程

启用最低血量限制
  -> 血量最低只降到临界值
  -> 不进入死亡流程
  -> 受击后仍然存活时，才尝试触发 Hostile -> Friendly
```

`OldManController` 只负责敌人组件缓存和配置注入，是否运行由 `NpcModeController` 统一控制。

`NpcMode`：

```text
类型：enum
定义来源：新增脚本
用途：描述 NPC 当前大模式

Friendly：友好状态，可以对话，可以简单移动，不能攻击。
Hostile：敌对状态，不能对话，可以追击、攻击、受击。
```

## 改动清单

### 新增

- `Assets/Scripts/Characters/Common/NpcMode.cs`
  - 定义 `Friendly`、`Hostile`。
  - 作为 NPC 当前大模式判断依据。

- `Assets/Scripts/Characters/Common/NpcModeController.cs`
  - 持有当前 `NpcMode`。
  - 持有 `_friendlyBehaviours`，由 Inspector 配置友好模式启用的组件。
  - 持有 `_hostileBehaviours`，由 Inspector 配置敌对模式启用的组件。
  - `_hostileBehaviours` 不配置 `StateMachineController` 或其子类。
  - 持有 `_hostileConditions`，类型为 `ConditionSO[]`，用于判断当前是否允许从友好切换敌对。
  - 持有 `_hostileQuestDefinition`，用于构造敌对判断的 `ConditionContext`。
  - 持有 `_friendlyConditions`，类型为 `ConditionSO[]`，用于判断当前是否允许从敌对切回友好。
  - 持有 `_friendlyQuestDefinition`，用于构造友好判断的 `ConditionContext`。
  - 持有 `_mover`，用于切换模式时停止移动。
  - 持有 `_stateMachineController`，用于启动或停止敌对状态机。
  - 通过 `ConditionUtility.AreAllMet()` 统一执行友好 / 敌对条件判断。
  - 负责切换友好 / 敌对状态。
  - 通过 `Behaviour[]` 批量启停友好组件和敌对组件。
  - 切换模式时调用 `Mover.StopMove()` 清理上一种模式留下的速度。
  - 切到 `Friendly` 时调用 `StateMachineController.StopMachine()`。
  - 切到 `Hostile` 时调用 `StateMachineController.StartMachine()`。
  - 提供 `SetMode(NpcMode mode)`。
  - 提供 `TrySwitchToHostile()`。
  - 提供 `TrySwitchToFriendlyAfterDefeat()`。

- `Assets/Scripts/Characters/Common/FriendlyPatrol.cs`
  - 负责友好状态下的简单来回移动。
  - 玩家进入停留范围时停止移动。
  - 玩家离开后继续巡逻。
  - 只处理友好移动，不处理对话和战斗。

### 修改

- `Assets/Scripts/Dialogue/DialogueNpcTrigger.cs`
  - 保持对话职责不变。
  - 增加对 `NpcModeController` 的可选判断。
  - 只有当前模式为 `Friendly` 时允许显示提示和触发对话。
  - 如果对象上没有 `NpcModeController`，继续按纯对话 NPC 处理。
  - 不接管敌对切换逻辑。

- `Assets/Scripts/Characters/StateMachine/Components/DamageableHealth.cs`
  - 在受击时可选检查对象上是否存在 `NpcModeController`。
  - 没有 `NpcModeController` 时，按普通玩家、Boss、纯战斗敌人的原受击流程处理。
  - 有 `NpcModeController` 且当前是 `Friendly` 时，先调用 `TrySwitchToHostile()`。
  - `TrySwitchToHostile()` 返回 `false` 时，本次攻击无效，不扣血，不触发受击硬直，不触发 `TakeDamage` 谓词。
  - `TrySwitchToHostile()` 返回 `true` 时，第一次攻击默认正常扣血。
  - 新增最低血量限制开关 `_limitMinHealth`，默认关闭，保持原死亡流程。
  - 新增最低血量配置 `_minHealth`，只在 `_limitMinHealth == true` 时生效。
  - 开启最低血量限制时，血量最低降到 `_minHealth`，不进入死亡流程。
  - 受击后如果对象仍然存活，再调用 `NpcModeController.TrySwitchToFriendlyAfterDefeat()`。
  - `NpcModeController` 只负责条件判断和模式切换，不关心健康系统是否开启最低血量限制。
  - 保持原有受击谓词能力。

- `Assets/Scripts/Characters/Enemies/OldMan/OldManController.cs`
  - 保持敌人配置注入逻辑。
  - 由 `NpcModeController` 控制相关敌对组件是否运行。

- `Assets/PlayArk/StateMachine/StateMachineController.cs`
  - 新增状态机运行标记 `_isRunning`。
  - 新增状态机绑定标记 `_isBound`，避免重复 Bind。
  - 新增自动启动配置 `_playOnStart`，默认 `true`。
  - 新增 `StartMachine()`，负责绑定状态机、进入入口状态、允许 `Update` / `FixedUpdate` 驱动。
  - 新增 `StopMachine()`，负责停止 `Update` / `FixedUpdate` 驱动。
  - `Start()` 中只有 `_playOnStart == true` 时自动调用 `StartMachine()`。
  - `Update` 和 `FixedUpdate` 在 `_isRunning == false` 时直接返回。
  - 玩家、普通敌人、Boss 等纯战斗角色保持 `_playOnStart = true`。
  - 友好 / 敌对混合 NPC 配置 `_playOnStart = false`，由 `NpcModeController` 调用 `StartMachine()`。
  - 不引用 `NpcModeController`。
  - 不查找 `NpcModeController`。
  - 不判断 NPC 友好 / 敌对模式。
  - 玩家、普通敌人、Boss 等纯战斗角色仍可只挂 `StateMachineController` 或其子类运行。

- `Assets/PlayArk/StateMachine/StateMachine.cs`
  - 新增 `MachineExit()`。
  - `MachineExit()` 只负责退出当前状态并清空当前状态引用。
  - `StateMachineController.StopMachine()` 可以调用它，让敌对状态停得更干净。

## 执行顺序

1. 新增 `NpcMode` 枚举。
2. 新增 `NpcModeController`，先实现组件启停和模式切换。
3. 新增 `FriendlyPatrol`，实现友好状态下的简单巡逻和遇到玩家停下。
4. 修改 `StateMachineController`，增加显式启动 / 停止状态机的方法。
5. 必要时修改 `StateMachine`，增加退出当前状态的方法。
6. 修改 `DialogueNpcTrigger`，有 `NpcModeController` 时只允许 `Friendly` 模式触发对话，没有时继续按纯对话 NPC 触发对话。
7. 修改 `NpcModeController`，增加敌对状态受击后未死亡时按条件切回友好的判断链。
8. 修改 `DamageableHealth`，支持友好 NPC 被攻击后按条件切换敌对；条件不满足时本次攻击完全无效。
9. 修改 `DamageableHealth`，增加最低血量限制；受击后仍然存活时才尝试触发敌对切回友好。
10. 修改或确认 `OldManController`、`OldManBrain`、`OldManActionDriver` 不承担对话职责。
11. 在代码层完成引用检查。

## 验证

引用检查：

- `rg "NpcMode" Assets/Scripts`
- `rg "NpcModeController" Assets/Scripts`
- `rg "FriendlyPatrol" Assets/Scripts`
- `rg "ConditionSO" Assets/Scripts Assets/PlayArk`
- `rg "ConditionUtility.AreAllMet" Assets/Scripts`
- `rg "DialogueNpcTrigger" Assets/Scripts`
- `rg "TrySwitchToHostile" Assets/Scripts`
- `rg "TrySwitchToFriendlyAfterDefeat" Assets/Scripts`
- `rg "StartMachine" Assets`
- `rg "StopMachine" Assets`

代码格式检查：

- `git diff --check`

Unity Console 检查：

- 检查 Console 是否有 Error / Exception。

运行流程验证：

1. 友好 NPC 默认可以来回移动。
2. 玩家靠近友好 NPC 后，NPC 停下并显示对话提示。
3. 玩家按交互键后，可以正常进入对话。
4. 友好 NPC 被玩家攻击后，进入敌对状态。
5. 敌对状态下不再显示对话提示，不能触发对话。
6. 敌对状态下可以追击、攻击玩家。
7. 普通敌对 NPC 血量归零后仍按原流程死亡。
8. 特殊敌对 NPC 开启最低血量限制时，被打到最低血量后不死亡。
9. 特殊敌对 NPC 受击后仍然存活，并且友好条件满足时，切回友好状态并可以重新对话。
10. 特殊敌对 NPC 受击后仍然存活，但友好条件不满足时，保持敌对状态。

## 禁止项

- 不修改 Prefab、Scene、ScriptableObject、`.asset`、`.unity`、`.meta` 等 Unity 资产文件，除非用户明确要求。
- 不顺手清理、恢复、补齐 `.meta`。
- 不把对话逻辑塞进敌人战斗状态机。
- 不用切换预制体来实现友好 / 敌对变化。
- 不在本计划中抽象完整 NPC 继承体系。
- 不把所有现有敌人和 NPC 一次性迁移。
- 不写“当前阶段 / 后续阶段”，规划文档只描述本计划本身。
- 不扩大用户要求的重构范围。
