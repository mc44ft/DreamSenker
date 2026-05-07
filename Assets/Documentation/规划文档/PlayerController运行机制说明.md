# PlayerController 运行机制说明

## 结论

`PlayerController` 是玩家对象的总入口，但它自己不直接写移动、跳跃、攻击、受击逻辑。

它主要负责四件事：

- 绑定玩家身上的核心组件。
- 从配置或存档里拿到玩家运行数据。
- 在无武装形态和剑形态之间切换。
- 接收状态机动作，把“切换形态”这类玩家级指令执行掉。

真正的移动、动画、受击、攻击配置，主要交给 `FormUnarmed` / `FormSword` 和玩家身上的功能组件处理。

---

## 相关脚本分工

### `PlayerController.cs`

位置：`Assets/Scripts/Character/Player/StateMachine/PlayerController.cs`

职责：

- 持有 `PlayerConfigSO`。
- 持有当前玩家存档数据 `PlayerSaveData`。
- 缓存 `PlayerHealth`、`Mover`、`InputReader`、`SpriteRenderer`、两个形态脚本。
- 控制双形态切换。
- 实现 `IAction`，让状态机可以调用 `SwitchFormHandle`。

### `StateMachineController.cs`

位置：`Assets/PlayArk/StateMachine/StateMachineController.cs`

职责：

- 持有状态机资源 `_stateMachine`。
- 运行时克隆状态机。
- `Start()` 时把控制器绑定给状态机。
- 每帧驱动状态机：
  - `Update()` 调 `LogicUpdate()`。
  - `FixedUpdate()` 调 `PhysicsUpdate()`。

### `FormStrategy.cs`

位置：`Assets/Scripts/Character/Player/StateMachine/FormStrategy/FormStrategy.cs`

职责：

- 作为“形态策略”的基类。
- 接收 `PlayerConfig`、当前形态配置、玩家存档数据。
- 切换动画控制器。
- 重置跳跃次数。
- 初始化血量。
- 把形态配置注入给 `Mover`、`Attacker`、`PlayerHealth` 等组件。

### `FormUnarmed.cs` / `FormSword.cs`

职责：

- 接收状态机发来的 `EAction`。
- 当前形态启用时才执行动作。
- 把动作转发给具体组件：
  - `Move` -> `Mover.Move()`
  - `JumpEnterSetup` -> `Mover.JumpEnterSetup()`
  - `FallEnterSetup` -> `Mover.FallEnterSetup()`
  - `PlayAnimation` -> `AnimationPlayer.PlayAnimation()`
  - `Knockback` -> `PlayerHealth.DoKnockback()`

---

## 玩家生成后的主流程

玩家不是场景里固定摆着的，而是由 `GameManager` 生成。

主流程在 `GameManager.InitializeGame()` 里：

1. 加载当前地图。
2. 从 `PlayerConfigSO.PlayerConfig.PlayerPrefab` 实例化玩家。
3. `DontDestroyOnLoad(player.gameObject)`，让玩家跨场景保留。
4. 把实例保存到 `GameManager.Player`。
5. 调用 `Player.Initialize(...)`。
6. 玩家初始化完成后：
   - 背包系统读取 `Player.PlayerSaveData.PackageData`。
   - 按地图配置设置阴影强度。
   - 打开主面板。
   - 触发血量 UI 更新。
   - 根据当前地图状态更新玩家形态和镜像。

所以 `PlayerController.Initialize()` 是玩家真正进入游戏逻辑前的外部初始化入口。

---

## `Awake()` 做了什么

`PlayerController.Awake()` 主要缓存组件：

```csharp
_health = GetComponent<PlayerHealth>();
_mover = GetComponent<Mover>();
_inputReader = GetComponent<InputReader>();
_spriteRenderer = GetComponent<SpriteRenderer>();
_formUnarmed = GetComponent<FormUnarmed>();
_formSword = GetComponent<FormSword>();
```

然后用配置里的默认数据先填一个临时值：

```csharp
if(_playerConfig != null)
    _playerSaveData = _playerConfig.PlayerSaveData;
```

这里的 `_playerSaveData = _playerConfig.PlayerSaveData` 只是临时默认值。
真正进入游戏时，`Initialize()` 里会调用 `LoadData()`，再用存档数据或配置默认数据覆盖它。

---

## `Initialize()` 做了什么

```csharp
public void Initialize(Action onFinished)
{
    LoadData();
    SwitchForm();
    onFinished?.Invoke();
}
```

流程很短：

1. `LoadData()`：读取玩家存档。
2. `SwitchForm()`：切换到初始形态。
3. `onFinished?.Invoke()`：通知 `GameManager` 玩家初始化完毕。

这个方法是“外部初始化”，不是 Unity 自动生命周期。

---

## 存档数据怎么进来

```csharp
RunningDataManager.Instance.LoadData(out PlayerSaveData playerSaveData);
```

如果没有存档：

```csharp
_playerSaveData = HelpUtilities.DeepCopy(_playerConfig.PlayerSaveData);
```

意思是：拿 `PlayerConfigSO` 里的默认可保存数据，深拷贝一份作为运行时数据。

如果有存档：

```csharp
_playerSaveData = playerSaveData;
```

意思是：直接使用磁盘里的 `PlayerSaveData.json`。

这里不能直接长期使用 `_playerConfig.PlayerSaveData`，否则运行时修改可能污染配置对象里的默认数据。现在无存档分支用了深拷贝，这个方向是对的。

---

## 双形态切换怎么跑

玩家有两个形态脚本：

- `_formUnarmed`：无武装形态。
- `_formSword`：剑形态。

当前形态由 `_currentForm` 保存。

### 第一次切换

```csharp
if (_currentForm == null)
{
    SwitchFormReal();
}
```

第一次初始化时，`_currentForm` 是空，所以进入 `SwitchFormReal()`。

`SwitchFormReal()` 做的事：

```csharp
_currentForm = _formUnarmed;
_formUnarmed.enabled = true;
_formSword.enabled = false;
```

然后注入无武装形态配置：

```csharp
_currentForm.SwitchSetup(
    _playerConfig.PlayerConfig,
    _playerConfig.FormConfigUnarmed,
    _playerConfig.PlayerSaveData);
```

### 再次切换

```csharp
if (_currentForm == _formUnarmed) SwitchFormMirror();
else SwitchFormReal();
```

当前是无武装形态，就切到剑形态。
当前是剑形态，就切回无武装形态。

### 剑形态切换

`SwitchFormMirror()` 做的事：

```csharp
_currentForm = _formSword;
_formUnarmed.enabled = false;
_formSword.enabled = true;
```

然后注入剑形态配置：

```csharp
_currentForm.SwitchSetup(
    _playerConfig.PlayerConfig,
    _playerConfig.FormConfigSword,
    _playerConfig.PlayerSaveData);
```

---

## 形态切换后的配置注入

`SwitchSetup()` 在 `FormStrategy` 里。

它会做这些事：

1. 保存玩家通用配置。
2. 保存当前形态配置。
3. 保存玩家存档数据。
4. 切换动画控制器。
5. 重置跳跃次数。
6. 初始化血量。
7. 给玩家身上的组件注入当前形态配置。

核心逻辑：

```csharp
_animationPlayer.SetRuntimeAnimatorController(formConfig.RuntimeAnimatorController);
_mover.ResetJumpCounter();
_health.Initialize(_formConfig.MaxHealthAmount, _playerSaveData.CurrentHealth);
InjectionConfig(formConfig);
```

所以切换形态不只是开关脚本，还会换一整套移动、血量、攻击、动画配置。

---

## 状态机怎么调用玩家动作

状态机里有 `ActionState`。

`ActionState` 在进入、更新、物理更新、退出状态时，会执行配置好的 `ActionData`。

它的调用方式是：

```csharp
foreach (var actionSender in _controller.GetComponents<IAction>())
{
    foreach (ActionData action in actions)
    {
        actionSender.DoAction(action.action, action.parameters);
    }
}
```

意思是：状态机会找到玩家对象身上所有实现了 `IAction` 的组件，然后把动作广播出去。

玩家身上实现了 `IAction` 的主要对象有：

- `PlayerController`
- `FormUnarmed`
- `FormSword`

所以同一个 `EAction` 会发给多个组件。

能不能真正执行，取决于组件自己怎么处理：

- `PlayerController` 只处理 `SwitchFormHandle`。
- `FormUnarmed` 和 `FormSword` 处理移动、跳跃、动画、受击等动作。
- 两个形态脚本开头都有 `if(!this.enabled) return;`，所以只有当前启用的形态会响应动作。

---

## `SwitchFormHandle` 怎么触发切形态

状态机如果发出：

```csharp
EAction.SwitchFormHandle
```

`PlayerController.DoAction()` 会进入：

```csharp
SwitchFormHandle(parameters[0]);
```

`SwitchFormHandle()` 再问 `InputReader`：

```csharp
if (_inputReader.CheckKeyCodePressed(buttomName))
{
    SwitchForm();
}
```

如果参数是 `"SwitchFormButtonDown"`，`InputReader` 会读取：

```csharp
InputManager.Instance.SwitchFormButtonDown
```

也就是说，切形态不是 `PlayerController.Update()` 自己轮询，而是状态机动作里配置了一个“检测切换形态按钮”的动作。

---

## 镜像分身和阴影

镜像分身和阴影强度现在不在 `PlayerController` 里处理。

它们被拆到可选组件：

```text
Assets/Scripts/Character/Player/Components/PlayerMirrorEffect.cs
```

`GameManager` 在玩家生成后缓存一次：

```csharp
_playerMirrorEffect = Player.GetComponent<PlayerMirrorEffect>();
```

之后遇到特殊地图逻辑时判空调用：

```csharp
_playerMirrorEffect?.SetMirrorActive(true);
_playerMirrorEffect?.SetShadowDarknessStrength(value);
```

所以镜像表现已经从玩家核心控制器里移出。

`GameManager.SpecialMapInitialize()` 会根据地图控制它：

- 进入 `MirrorMap1`：打开镜像。
- 进入 `MirrorMap2`：关闭镜像，并调用 `Player.SwitchForm()`。

地图切换时，`GameManager` 会读取地图配置里的 `PlayerShadowDarknessStrength`，再设置到玩家阴影材质上。

---

## 一句话流程图

```text
GameManager 实例化 Player
    -> PlayerController.Awake 缓存组件
    -> GameManager 缓存可选 PlayerMirrorEffect
    -> GameManager 调 Player.Initialize
    -> LoadData 读取 PlayerSaveData
    -> SwitchForm 切到无武装形态
    -> FormStrategy.SwitchSetup 注入形态配置
    -> StateMachineController.Start 绑定并启动状态机
    -> ActionState 广播 EAction
    -> 当前启用的 FormStrategy 执行移动/动画/受击
    -> PlayerController 只处理切形态检测
```

---

## 当前代码里最容易误解的点

### 1. `PlayerController` 不是移动控制器

名字叫 `PlayerController`，但它不直接控制移动。

移动实际在：

- `Mover`
- `FormUnarmed`
- `FormSword`
- 状态机 `ActionState`

`PlayerController` 更像“玩家总协调器”。

### 2. 状态机动作是广播，不是精准调用

`ActionState` 会把动作发给所有 `IAction` 组件。

所以每个 `IAction` 都要自己判断：

- 这个动作我认不认识。
- 我当前是否启用。
- 参数够不够。

这个设计灵活，但出错时不容易追踪。

### 3. `SwitchFormReal` / `SwitchFormMirror` 命名有点绕

现在逻辑是：

- `SwitchFormReal()` -> 无武装形态。
- `SwitchFormMirror()` -> 剑形态。

从代码结果看，它不是单纯“现实/镜像”，而是“无武装/剑”的切换。后面如果继续维护，建议名字改成 `SwitchToUnarmedForm()` / `SwitchToSwordForm()`，更直接。

### 4. `SwitchSetup()` 传入的存档数据疑似用错

`PlayerController.LoadData()` 会把最终存档放到 `_playerSaveData`。

但当前 `SwitchFormReal()` 和 `SwitchFormMirror()` 传的是：

```csharp
_playerConfig.PlayerSaveData
```

不是：

```csharp
_playerSaveData
```

这意味着形态组件初始化血量、攻击倍率、背包引用时，可能拿到的是配置默认数据，而不是实际存档数据。

这个点建议后续单独确认。单看代码，我倾向认为这里应该传 `_playerSaveData`。

### 5. 父类和子类都有 `Awake()`，这里有潜在风险

`StateMachineController` 里有自己的 `private void Awake()`，用于克隆运行时状态机。

`PlayerController` 里也有自己的 `private void Awake()`。

这种写法不够稳。更清晰的写法应该是父类提供：

```csharp
protected virtual void Awake()
```

子类写：

```csharp
protected override void Awake()
{
    base.Awake();
    ...
}
```

这样才能明确保证状态机克隆逻辑一定执行。

---

## 推荐理解方式

把玩家拆成三层看：

### 第一层：状态机

决定“现在是什么状态”，例如待机、移动、跳跃、下落、受击。

### 第二层：形态策略

决定“当前形态下，同一个动作应该用哪套配置执行”。

例如无武装和剑形态可以有不同动画、攻击范围、移动参数。

### 第三层：功能组件

真正干活：

- `Mover` 负责速度、跳跃、朝向、接地检测。
- `AnimationPlayer` 负责播放动画。
- `Attacker` 负责攻击检测。
- `PlayerHealth` 负责血量和受击表现。

`PlayerController` 本身站在这三层外面，负责把它们接起来。镜像这类特殊地图表现不属于这三层，应该继续放在独立组件里。
