# MapSystem 方案对比

**制定日期**: 2026-04-23
**目的**: 对比两套去 `MapGraph` 化方案，并给出明确推荐。

---

## 一、背景

当前 `MapSystem` 的主要问题不是“不能用”，而是“结构不值当”：

- 读档走 `SaveMapSceneName`
- 场景切换走 `CurrentMapNodeGuid / PreviousMapNodeGuid`
- 地图配置依赖 `MapGraph -> MapNode -> MapData`
- 运行时身份和编辑器数据绑得太死

所以这次不讨论继续保留 `MapGraph`。  
只比较两套新方案：

1. **连接表驱动方案**
2. **点位直连方案**

---

## 二、方案 A：连接表驱动

### 2.1 核心思路

地图本身只描述“自己是谁”。  
地图之间怎么连，不写在地图里，而是单独放在一张“连接表”里。

也就是说：

- `MapDefinitionSO` 只管地图基础信息
- `MapConnectionDatabaseSO` 统一存所有地图连接关系
- 场景里的切图点只负责告诉系统：我属于哪条连接、我是这条连接的哪一端

### 2.2 数据结构

#### `MapDefinitionSO`

每张地图一个资源，只存地图自身数据：

```csharp
[CreateAssetMenu(menuName = "Map/Map Definition")]
public class MapDefinitionSO : ScriptableObject
{
    public string MapId;
    public string SceneName;
    public float PlayerShadowDarknessStrength = 2.5f;
}
```

#### `MapEndpointData`

表示连接的一端：

```csharp
[System.Serializable]
public struct MapEndpointData
{
    public string MapId;
    public string PointId;
}
```

#### `MapConnectionData`

表示一条完整连接：

```csharp
[System.Serializable]
public struct MapConnectionData
{
    public string ConnectionId;
    public MapEndpointData EndA;
    public MapEndpointData EndB;
}
```

#### `MapConnectionDatabaseSO`

统一管理所有连接：

```csharp
[CreateAssetMenu(menuName = "Map/Connection Database")]
public class MapConnectionDatabaseSO : ScriptableObject
{
    public List<MapConnectionData> Connections;
}
```

#### `MapTransitionPoint`

场景里的切图点组件：

```csharp
public class MapTransitionPoint : MonoBehaviour
{
    public string ConnectionId;
    public string PointId;
    public Sprite LoadingSprite;
    public float LoadingTime = 0.5f;
}
```

### 2.3 运行时流程

1. 玩家进入某个 `MapTransitionPoint`
2. 系统拿到它的 `ConnectionId + PointId`
3. 在 `MapConnectionDatabaseSO` 里找到这条连接
4. 判断当前点是 `EndA` 还是 `EndB`
5. 取另一端作为目标
6. 加载目标地图
7. 根据目标端的 `PointId` 找到目标落点
8. 把玩家放到对应位置

### 2.4 优点

- 不依赖方向
- 连接关系集中管理
- 模型统一，理论上更规范
- 适合复杂地图网络
- 以后一个点连多个特殊场景，也更容易扩展

### 2.5 缺点

- 概念多
- 需要理解“连接”“端点”“另一端”这些关系
- 配置时要同时维护连接表和场景点位
- 排错时要跨两层数据查问题
- 对初学者不友好

### 2.6 适用场景

更适合下面这些情况：

- 地图数量很多
- 联通关系复杂
- 有大量非方向型出口
- 需要比较强的结构一致性

如果地图不多，这套会显得有点重。

---

## 三、方案 B：点位直连

### 3.1 核心思路

不搞全局连接表。  
每个切图点直接写清楚：

- 我要去哪个地图
- 过去后落到哪个点

这套是最直白的：

- 地图自身信息放 `MapDefinitionSO`
- 场景里的切图点直接配置目标
- 场景里的落点单独挂一个标记组件

### 3.2 数据结构

#### `MapDefinitionSO`

还是每张地图一个资源：

```csharp
[CreateAssetMenu(menuName = "Map/Map Definition")]
public class MapDefinitionSO : ScriptableObject
{
    public string MapId;
    public string SceneName;
    public float PlayerShadowDarknessStrength = 2.5f;
}
```

#### `MapTransitionPoint`

场景里的切图点：

```csharp
public class MapTransitionPoint : MonoBehaviour
{
    public string PointId;
    public MapDefinitionSO TargetMap;
    public string TargetPointId;
    public Sprite LoadingSprite;
    public float LoadingTime = 0.5f;
}
```

#### `MapSpawnPoint`

场景里的落点标记：

```csharp
public class MapSpawnPoint : MonoBehaviour
{
    public string PointId;
}
```

### 3.3 运行时流程

1. 玩家进入 `MapTransitionPoint`
2. 组件直接给出：
   - `TargetMap`
   - `TargetPointId`
3. 系统加载目标地图
4. 在目标场景中查找 `PointId == TargetPointId` 的 `MapSpawnPoint`
5. 把玩家传到那个位置

### 3.4 优点

- 最直观
- 最容易理解
- 配置量最少
- 不需要再理解连接表和双端点
- 调试最简单
- 最适合地图数量不多的项目
- 对初学者最友好

### 3.5 缺点

- 双向连接要手动各配一次
- 多个点通向同一目标时，信息会有一点重复
- 如果以后地图数量暴涨，统一管理性不如连接表

### 3.6 适用场景

更适合下面这些情况：

- 当前地图数量不多
- 重点是先跑通
- 需要快速迭代
- 使用者还不想被复杂抽象绕晕

这套对你现在的项目状态更合适。

---

## 四、两套方案正面对比

| 对比项 | 连接表驱动方案 | 点位直连方案 |
|------|------|------|
| 理解成本 | 高 | 低 |
| 配置成本 | 高 | 低 |
| 出错概率 | 中到高 | 中 |
| 调试难度 | 高 | 低 |
| 扩展性 | 强 | 中等 |
| 数据一致性 | 强 | 中等 |
| 当前项目适配度 | 一般 | 高 |
| 初学者友好度 | 低 | 高 |
| 当前阶段推荐度 | 一般 | 高 |

---

## 五、你现在最适合哪套

**明确推荐：点位直连方案。**

理由很直接：

1. 你现在最缺的不是“最优雅的抽象”，而是“最不容易配错的结构”
2. 你当前地图规模不大，没必要上连接表这种更重的方案
3. `TargetMap` 直接用 SO 引用，已经能大幅降低配置错误
4. 只留下少量 `PointId` 字符串，风险是可控的
5. 以后真有需要，再从点位直连升级到连接表驱动也不晚

所以别想太多。  
先上最容易跑通、最容易维护的版本。

---

## 六、你担心的“字符串容易写错”，怎么处理

这个担心是对的，但可以压下去。

### 6.1 不要在点位上重复写当前地图 ID

**不要配。**

当前地图是谁，运行时本来就知道。  
每个点位再写一次，只会增加错误率。

### 6.2 地图目标不要用字符串

`TargetMap` 必须直接用 `MapDefinitionSO` 引用。

这样：

- 不需要手写地图 ID
- 拖拽就能配
- 少一类低级错误

### 6.3 只保留最少量字符串

真正保留字符串的地方只需要两个：

- 当前点自己的 `PointId`
- 目标落点的 `TargetPointId`

也就是说：

- 地图靠 SO 引用
- 点位靠字符串标识

这样已经是很克制的做法了。

### 6.4 点位命名规范固定

推荐统一命名：

- `Door_Left_01`
- `Door_Right_01`
- `BossExit_01`
- `Teleport_A`

别写太花，也别混中文拼音英文缩写乱命名。  
统一一点，后面自己看也舒服。

---

## 七、不推荐的做法

### 7.1 不推荐在点位上写“我是出口还是入口”

这个字段意义不大。

因为：

- 从 A 图看是出口
- 从 B 图看就是入口

这不是稳定数据，只是视角不同。  
存这个字段，收益很低。

### 7.2 不推荐现在就上复杂连接模型

连接表驱动不是错。  
但对你现在这个阶段，偏重了。

如果你现在强行上这套，大概率会发生两件事：

1. 配置时老怕写错
2. 出问题时自己不知道去哪查

这才是真正拖慢开发的东西。

---

## 八、最终建议

现在这件事，别追求一步到位的“高级架构”。  
你最该做的是选一个：

- 容易理解
- 容易配置
- 容易调试
- 不容易把自己绕晕

的方案。

所以最终建议就是：

1. 地图基础信息放 `MapDefinitionSO`
2. 切图点用 `MapTransitionPoint`
3. 落点用 `MapSpawnPoint`
4. `MapTransitionPoint` 直接配置：
   - `TargetMap`
   - `TargetPointId`
5. 不再依赖方向
6. 不再依赖 `MapGraph`
7. 暂时不要上连接表

这套最适合你现在的项目。

先把它做对，再谈进阶。
