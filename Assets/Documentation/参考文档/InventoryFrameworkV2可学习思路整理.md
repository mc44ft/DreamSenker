# InventoryFrameworkV2 可学习思路整理

## 结论

Inventory Framework V2 不适合直接照搬到当前项目：它基于 UI Toolkit，功能覆盖装备、商店、箱子、制作、热键栏等，明显大于 DreamSenker 当前背包需求。

但它有几件事很值得学：

- 用 `Store` 收口所有背包修改。
- 把物品配置和背包存档数据拆开。
- 用 `Stack` 表达“同类物品 + 数量”。
- UI 只做显示和输入，不直接改背包数据。
- 输入先转事件，再交给背包逻辑处理。
- 示例拆得很小，按复杂度逐步扩展。

DreamSenker 当前目标应该是“小而清楚”：

```text
格子展示
  -> 同类道具数量堆叠
  -> 道具 / 任务物品两个标签页
  -> 可使用道具
  -> 任务道具
  -> 不做拖拽
  -> 不做装备、商店、制作
```

## 参考来源

- Inventory Framework V2 首页：`https://gamedevsimplified.github.io/docs/`
- Getting Started：`https://gamedevsimplified.github.io/docs/docs/getting-started/`
- Add new items：`https://gamedevsimplified.github.io/docs/guides/add-new-items/`
- New Input System：`https://gamedevsimplified.github.io/docs/guides/new-input-system/`

## 1. 学 Store：所有背包修改只走一个入口

Inventory Framework V2 的示例结构里通常包含 `Scene / UI Document / Controller / Store`。其中最值得学的是 `Store`：它不是 UI，也不是存档本身，而是背包状态修改中心。

它的核心思路是：

```text
外部输入
  -> 发送事件 / 调用 Store
      -> Store 判断规则
          -> 修改背包状态
              -> 通知 UI 刷新
```

这点正好对应 DreamSenker 当前问题：现在 `PackagePanel`、`TaskPanel`、`ItemPickUp` 都能直接改背包。

当前调用关系：

```text
ItemPickUp
  -> InventoryManager.AddItemToPackage()

PackagePanel
  -> InventoryManager.RemoveItemFromPackage()
  -> EventCenter.Game_BonusEffect

TaskPanel
  -> InventoryManager.RemoveItemFromPackage(EPackageItemType.Chen)
```

更好的方向：

```text
ItemPickUp
  -> InventoryManager.TryAddItem()

PackagePanel
  -> InventoryManager.TryUseItem()

TaskPanel
  -> InventoryManager.TryRemoveItem()
```

也就是说，`InventoryManager` 应该升级成当前项目里的小型 `Store`。

推荐接口：

```csharp
public bool HasItem(EPackageItemType type, int count = 1);
public bool TryAddItem(EPackageItemType type, int count = 1);
public bool TryRemoveItem(EPackageItemType type, int count = 1);
public bool TryUseItem(EPackageItemType type);
public bool CanUseItem(EPackageItemType type);
public IReadOnlyList<InventoryItemStack> GetItemStacks();
public IReadOnlyList<InventoryItemStack> GetItemStacks(EPackageItemCategory category);
```

这样做的好处：

- UI 不再直接碰 `PackageData.PackageDict`。
- 任务系统不再盲目扣物品。
- 拾取失败时以后能返回 false。
- 可使用道具和任务道具的规则有统一入口。
- 标签页过滤由背包数据层提供，不让 UI 自己乱筛字典。

不要学过头：

- 不需要做完整事件总线式 Store。
- 不需要做可撤销状态、复杂命令系统。
- 不需要为了背包引入完整函数式架构。

DreamSenker 只需要一个清晰的 `InventoryManager`。

## 2. 学 ItemBase：物品配置和数量数据分开

插件的 `Add new items` 文档里，新增物品时会定义类似 `ItemBase` 的基础信息：`Id`、`Name`、`Icon`、`Stack`、`Class`。这套思路可以直接简化进当前项目。

DreamSenker 已经有类似结构：

```text
PackageItemConfigSO
  -> PackageItemInfo[]

PackageItemInfo
  -> EPackageItemType
  -> Icon
  -> Description
  -> BonusEffectType
  -> BonusAmount
```

当前缺的是两个关键字段：

```text
Category
  -> Usable
  -> Quest

MaxStack
  -> 单格最大堆叠数量
```

推荐配置结构：

```csharp
public enum EPackageItemCategory
{
    Usable,
    Quest,
}

public class PackageItemInfo
{
    public EPackageItemType ItemType;
    public EPackageItemCategory Category;
    public int MaxStack = 99;
    public Sprite Icon;
    public string Description;
    public EBonusEffectType BonusEffectType;
    public int BonusAmount;
}
```

这里的重点不是字段名，而是分清两种数据：

```text
配置数据
  -> 这个物品是什么
  -> 图标是什么
  -> 能不能使用
  -> 使用后有什么效果
  -> 最大堆叠数是多少

存档数据
  -> 玩家现在有几个
```

当前 `PackageData` 可以继续保持简单：

```csharp
public class PackageData
{
    public Dictionary<EPackageItemType, int> PackageDict = new();
}
```

这就够了。不要为了“像插件一样专业”把简单道具做成复杂物品实例。

## 3. 学 Stack：一个格子显示一类物品和数量

插件里有 `Stack` 思想：物品不是一个个散开显示，而是一组同类物品叠在一起。

这点对当前项目最有用。

当前问题：

```text
RedFruit x 5
  -> 生成 5 个 PackagePanelItemIcon
```

应该改成：

```text
RedFruit x 5
  -> 生成 1 个 PackagePanelItemIcon
  -> 右下角显示 5
```

建议加一个只读视图数据：

```csharp
public readonly struct InventoryItemStack
{
    public EPackageItemType ItemType { get; }
    public int Count { get; }
    public PackageItemInfo Info { get; }
}
```

`InventoryManager.GetItemStacks()` 负责把存档字典转成 UI 可用数据：

```text
PackageData.PackageDict
  -> InventoryItemStack[]
      -> PackagePanel 刷新格子
```

标签页过滤也应该走同一层：

```text
PackagePanel 当前标签页
  -> EPackageItemCategory.Usable
      -> InventoryManager.GetItemStacks(Usable)

PackagePanel 当前标签页
  -> EPackageItemCategory.Quest
      -> InventoryManager.GetItemStacks(Quest)
```

不要让 `PackagePanel` 自己遍历 `PackageDict` 后再判断 `PackageItemInfo.Category`。UI 可以知道当前标签页是什么，但不应该知道背包字典怎么组织。

这样 `PackagePanel` 不需要知道字典结构，也不需要自己查配置。

UI 格子结构建议：

```text
PackagePanelItemIcon
  -> Icon
  -> CountText
  -> SelectedState
  -> ItemType
```

数量显示规则：

- `Count <= 1`：可以隐藏数量文本。
- `Count > 1`：显示数量。
- 任务道具也显示数量，因为任务道具也可能是复数。

使用道具后的刷新：

```text
InventoryManager.TryUseItem(type)
  -> 检查是否可使用
  -> 检查数量是否足够
  -> 触发道具效果
  -> 数量 -1
  -> 如果为 0，从字典移除
  -> PackagePanel 刷新当前格子或重建格子列表
```

这就是小巧精致的关键：**数据简单，显示明确。**

## 4. 学分类视图：标签页只是过滤视图，不是两套背包

你的背包需要两个标签页：

```text
道具
  -> 显示 Category = Usable 的物品

任务物品
  -> 显示 Category = Quest 的物品
```

这里容易犯一个错误：为两个标签页做两份背包数据。

不要这样：

```text
PackageData
  -> UsableDict
  -> QuestDict
```

更好的做法是：存档仍然只保留一份数量数据，标签页只是按配置分类过滤出来的视图。

```text
PackageData.PackageDict
  -> InventoryManager.GetItemStacks(Usable)
      -> 道具标签页

PackageData.PackageDict
  -> InventoryManager.GetItemStacks(Quest)
      -> 任务物品标签页
```

原因：

- 添加物品时不用判断该写入哪份数据。
- 存档结构更简单。
- 物品从任务物品改成可使用道具时，只改配置，不迁移存档。
- UI 标签页切换只是刷新视图，不影响真实数据。

`PackagePanel` 里只需要保存当前标签页：

```csharp
private EPackageItemCategory _currentCategory = EPackageItemCategory.Usable;
```

标签按钮逻辑：

```text
点击“道具”
  -> _currentCategory = Usable
  -> RefreshCurrentTab()

点击“任务物品”
  -> _currentCategory = Quest
  -> RefreshCurrentTab()
```

刷新逻辑：

```text
RefreshCurrentTab()
  -> InventoryManager.GetItemStacks(_currentCategory)
  -> 重建或刷新格子
  -> 清空当前选中
  -> 如果没有物品，显示当前标签页空状态
```

空状态文案建议：

```text
道具标签页为空：暂无可使用道具
任务物品标签页为空：暂无任务物品
```

这比所有东西堆在一个列表里清楚，也比做复杂筛选系统轻。

## 5. 学 View 分离：UI 不直接做业务

Inventory Framework V2 的文档强调 UI、Controller、Store 分开。当前项目也应该学这个边界。

当前 `PackagePanel` 的问题是它做了三件事：

```text
显示格子
  + 扣背包数量
  + 触发道具效果
```

建议改成：

```text
PackagePanel
  -> SwitchTab(category)
  -> Refresh(category)
  -> SelectItem(stack)
  -> UseSelectedItem()
      -> InventoryManager.TryUseItem(selectedType)
```

`PackagePanel` 只关心：

- 当前处在哪个标签页。
- 当前选中了哪个格子。
- 描述文本显示什么。
- 确认按钮是否可见。
- 数量文本怎么显示。

`PackagePanel` 不应该关心：

- 物品是否任务道具。
- 使用后发什么事件。
- 数量怎么扣。
- 任务道具能不能被消耗。

这些都应该让 `InventoryManager` 或后续的 `ItemEffectResolver` 判断。

推荐关系：

```text
PackagePanel
  -> InventoryManager.GetItemStacks(currentCategory)
  -> InventoryManager.CanUseItem(type)
  -> InventoryManager.TryUseItem(type)
```

不是：

```text
PackagePanel
  -> InventoryManager.PackageData.PackageDict
  -> InventoryManager.RemoveItemFromPackage()
  -> EventCenter.Game_BonusEffect
```

这一步做完，未来就算换 UI Toolkit 插件，业务规则也不用重写。

## 6. 学 Event：输入和结果用事件解耦

插件的新输入系统文档里，输入脚本不是直接操作 UI 或数据，而是发布事件，再由 Store 处理。

DreamSenker 已经有类似结构：

```text
InputManager
  -> EventCenter.InputUI_PackagePanel
      -> GamePlayUiController
          -> UIManager.ShowPanel<PackagePanel>()
```

这层是对的，可以保留。

背包内部也可以补两个轻量事件：

```text
Inventory_ItemChanged
Inventory_ItemPicked
```

用途：

```text
ItemPickUp
  -> InventoryManager.TryAddItem(type)
  -> Inventory_ItemPicked

PackagePanel
  -> 监听 Inventory_ItemChanged
  -> 刷新格子数量

任务 / 条件模块
  -> 监听 Inventory_ItemPicked
  -> 处理 Chen 这类剧情状态
```

但不要一上来事件化所有东西。当前最小做法是：

- `PackagePanel` 打开时主动刷新。
- 使用道具成功后手动刷新当前面板。
- 后续需要实时刷新时，再加 `Inventory_ItemChanged`。

## 7. 学示例拆分：从最小版本开始，不要一次吃完整功能

插件文档建议从 demo/example 入手，并且示例按复杂度拆分。这个思路值得学。

对 DreamSenker 来说，背包也应该拆成几个小版本：

### V1：收口数据入口

目标：

- `InventoryManager` 增加 `HasItem()`、`TryAddItem()`、`TryRemoveItem()`。
- `PackagePanel` 不直接改字典。
- `TaskPanel` 交任务前先检查道具是否足够。

结果：

- 背包数据不会被各处随便改。

### V2：格子堆叠显示

目标：

- 增加 `InventoryItemStack`。
- 一种物品一个格子。
- `PackagePanelItemIcon` 增加数量文本。

结果：

- 复数道具显示正常。
- 背包从“临时列表”变成真正的格子背包。

### V3：道具分类和使用规则

目标：

- `PackageItemInfo` 增加 `Category`。
- `CanUseItem()` 控制使用按钮。
- 任务道具不可使用。
- 可使用道具走统一使用流程。
- `PackagePanel` 增加“道具 / 任务物品”两个标签页。
- 当前标签页只显示对应 `Category` 的格子。

结果：

- 不再特判 `Chen`。
- 新增任务道具不用改 UI。
- 玩家能清楚区分消耗道具和任务物品。

### V4：视觉精修

目标：

- 选中态更清楚。
- 标签页选中态更清楚。
- 空背包状态更干净。
- 每个标签页有自己的空状态文案。
- 数量文本位置统一。
- 描述区域显示物品名、描述、效果。
- 使用成功后只刷新变化格子。

结果：

- 小系统也有完成度。

## 8. 当前项目不要学的内容

不要学这些：

- 装备系统。
- 商店系统。
- 制作系统。
- 箱子 / 仓库系统。
- 拖拽换位。
- UI Toolkit 全量迁移。
- 新输入系统强制迁移。
- 大型 Store Bus 框架。

原因很简单：这些不是当前目标，会拖慢你真正要做的内容。

当前背包做精致，靠的不是功能多，而是边界干净：

```text
数据入口干净
  -> 标签页分类明确
  -> 一种物品一个格子
  -> 数量显示清楚
  -> 任务道具不能误用
  -> 使用道具反馈明确
  -> UI 不直接碰存档字典
```

## 推荐最终结构

```text
PackageData
  -> Dictionary<EPackageItemType, int>

PackageItemConfigSO
  -> PackageItemInfo[]
  -> runtime Dictionary<EPackageItemType, PackageItemInfo>

InventoryManager
  -> SetupData()
  -> HasItem()
  -> TryAddItem()
  -> TryRemoveItem()
  -> TryUseItem()
  -> CanUseItem()
  -> GetItemStacks(category)

InventoryItemStack
  -> ItemType
  -> Count
  -> PackageItemInfo

PackagePanel
  -> SwitchTab(category)
  -> Refresh()
  -> SelectItem()
  -> UseSelectedItem()

PackagePanelItemIcon
  -> SetIcon()
  -> SetCount()
  -> SetSelected()
```

## 最小可执行改造清单

1. `PackageItemInfo` 增加 `Category` 和 `MaxStack`。
2. `InventoryManager` 增加 `HasItem()`、`TryAddItem()`、`TryRemoveItem()`。
3. 新增 `InventoryItemStack`，由 `InventoryManager.GetItemStacks()` 返回。
4. `PackagePanel` 改为每种物品生成一个格子。
5. `PackagePanelItemIcon` 增加数量文本。
6. `PackagePanel` 用 `CanUseItem()` 控制使用按钮。
7. `PackagePanel` 增加“道具 / 任务物品”两个标签页。
8. `PackagePanel` 切换标签页时调用 `GetItemStacks(category)` 刷新对应格子。
9. `InventoryManager.TryUseItem()` 负责扣数量和触发道具效果。
10. `TaskPanel` 交任务前检查并通过 `TryRemoveItem()` 扣任务道具。
11. `ItemPickUp` 只负责拾取，不直接改剧情状态。

这套改完，你的背包就够用了：小、清楚、能扩展，视觉上也能做得精致。
