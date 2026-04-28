# MapSystem 配置体验优化规划

**制定日期**: 2026-04-23
**目标**: 保留连接表驱动架构，但去掉手写 ID 和重复配置，让地图切换系统更适合长期维护和简历展示。

---

## 一、问题结论

当前系统能正常运行，但配置体验差：

1. 地图要手写 `MapId`
2. 点位要手写 `PointId`
3. 旧方案还需要维护 `ConnectionId`
4. 点位信息要在场景和连接表里重复配置

结论：

1. 保留 `MapDefinitionSO + MapConnectionDatabaseSO + MapLinkPoint`
2. 删除普通连接的 `ConnectionId`
3. `MapId / PointGuid` 自动生成
4. 连接表通过自定义 Inspector 下拉选择点位
5. 新增 `MapRegistrySO` 管理全部地图

---

## 二、核心设计

### 2.1 `MapDefinitionSO`

职责：描述一张地图。

保留字段：

1. `MapId`
2. `SceneName`
3. `PlayerShadowDarknessStrength`

调整：

1. `MapId` 仅在字段为空时自动生成
2. Inspector 只读显示
3. 生成后不自动变化
4. 不让用户手写字符串 ID

### 2.2 `MapLinkPoint`

职责：描述场景里的一个地图连接点。

建议字段：

1. `PointGuid`
2. `DisplayName`

规则：

1. `PointGuid` 仅在字段为空时自动生成，运行时使用
2. `DisplayName` 给人看，用于 Inspector 下拉显示
3. `DisplayName` 可以不填，但 Inspector 下拉不能显示空白
4. `PointGuid` 生成后不自动变化
5. 不再配置 `ConnectionId`

下拉显示规则：

1. `DisplayName` 有值：显示 `DisplayName (PointGuid前8位)`
2. `DisplayName` 为空：显示 `未命名点 (PointGuid前8位)`
3. `PointGuid` 永远参与显示，避免多个点重名后无法区分
4. `DisplayName` 不参与运行时逻辑，只是编辑器可读标签

### 2.3 `MapConnectionDatabaseSO`

职责：保存地图点位之间的连接关系。

建议结构：

```csharp
[CreateAssetMenu(fileName = "MapConnectionDatabase_", menuName = "Game/Map/Map Connection Database")]
public class MapConnectionDatabaseSO : ScriptableObject
{
    public MapRegistrySO MapRegistry;
    public List<MapConnectionData> Connections;
    public List<MapPointScanCache> PointScanCaches;
}

[Serializable]
public class MapConnectionData
{
    public string ConnectionDisplayName;
    public MapEndpointData EndA;
    public MapEndpointData EndB;
}

[Serializable]
public class MapEndpointData
{
    public string MapId;
    public string PointGuid;
}

[Serializable]
public class MapPointScanCache
{
    public string MapId;
    public List<MapPointScanData> Points;
}

[Serializable]
public class MapPointScanData
{
    public string PointGuid;
    public string DisplayName;
}
```

关键决策：

1. 删除 `ConnectionId`
2. 普通连接身份由 `EndA + EndB` 决定
3. `ConnectionDisplayName` 只给人看，不参与运行时查找
4. `MapConnectionDatabaseSO` 持有唯一的 `MapRegistrySO`
5. `GameManager` 持有 `MapConnectionDatabaseSO`，不单独配置 `MapRegistrySO`
6. 运行时和连接表 Inspector 都通过这份 `MapRegistrySO` 获取地图清单
7. `PointScanCaches` 是编辑器生成缓存，用于 Inspector 下拉，不作为运行时连接身份

### 2.4 `MapRegistrySO`

职责：保存地图系统的全部地图清单。

建议字段：

```csharp
public List<MapDefinitionSO> AllMaps;
```

用途：

1. 运行时通过它查找地图
2. Editor 扫描工具通过它知道哪些场景是地图场景
3. 不再依赖 `Build Settings`
4. 不再依赖主场景里的 `GameManager.MapDefinitions`
5. 只在 `MapConnectionDatabaseSO` 中配置一次

---

## 三、运行时规则

普通地图切换流程：

1. 玩家触发 `MapTransitionTrigger`
2. 读取当前点位的 `PointGuid`
3. 读取当前地图 `CurrentMapId`
4. 用 `CurrentMapId + PointGuid` 在连接表里反查连接
5. 找到唯一连接后，取另一端作为目标地图和落点

错误规则：

1. 找不到连接：直接报错
2. 匹配到多条连接：直接报错
3. 不默认返回第一条
4. 不做 fallback
5. 运行时加载连接表时，对致命错误再做一次防御性检查
6. 防御性检查失败：直接报错，不继续切图

原因：

1. 返回第一条会依赖连接表顺序
2. Inspector 排序变化会改变游戏行为
3. 这种隐性 bug 不值得留

---

## 四、连接表 Inspector 方案

目标：用户不手写 `MapId / PointGuid`。

配置流程：

1. 在连接表顶部配置一次 `MapRegistrySO`
2. Inspector 顶部提供一个 `扫描全部点位` 按钮
3. 用户点击 `扫描全部点位` 后，Editor 遍历 `MapRegistrySO.AllMaps`
4. 对每个 `MapDefinitionSO`，Editor 读取 `SceneName`
5. Editor 以 additive 方式临时加载目标场景，扫描 `MapLinkPoint`
6. 扫描完成后立即卸载目标场景，并恢复 `active scene / selection`
7. 所有扫描结果写入 `MapConnectionDatabaseSO.PointScanCaches`
8. `EndA / EndB` 的地图选择是下拉框，选项来自 `MapRegistrySO.AllMaps`
9. `EndA / EndB` 的点位选择是下拉框，选项来自当前地图对应的 `PointScanCaches`
10. 连接表保存稳定的 `MapId + PointGuid`

Inspector 预期效果：

```text
EndA:
  Map: [Camp]
  Point: [Camp_To_Cave_Exit (7f3a2c91)]

EndB:
  Map: [Cave]
  Point: [未命名点 (a91b44e0)]
```

需要的 Unity Editor 技术：

1. `CustomEditor`
2. `EditorGUILayout.Popup`
3. `EditorSceneManager.OpenScene`
4. `FindObjectsByType<MapLinkPoint>`
5. `SerializedProperty`
6. `Undo.RecordObject`
7. `EditorUtility.SetDirty`

扫描规则：

1. `MapRegistrySO` 为空时，地图下拉、点位下拉和 `扫描全部点位` 按钮禁用
2. 扫描按钮只有一个，放在连接表 Inspector 顶部
3. 扫描方式为 additive 临时加载，不替换当前场景
4. 扫描完成后立即卸载目标场景，并恢复 `active scene / selection`
5. 扫描工具只读场景，不修改场景
6. 扫描工具不自动生成 `PointGuid`
7. 扫描工具不自动保存场景
8. 空 `PointGuid` 的 `MapLinkPoint` 不进入点位下拉，只显示黄色警告
9. 如果重新扫描后，原来选中的点已不存在，保留原值并标红报错，不自动清空
10. 如果场景里点位后来有增删改，不依赖静默同步，用户自己点击 `扫描全部点位` 刷新缓存
11. `PointScanCaches` 是生成缓存，重新扫描时可以整体刷新

---

## 五、实施步骤

1. 新增 `MapRegistrySO`
2. 修改 `MapConnectionDatabaseSO`，让它持有一个 `MapRegistrySO`
3. 修改 `GameManager`，让它持有 `MapConnectionDatabaseSO`
4. 将运行时地图查找的唯一数据源收口到 `MapConnectionDatabaseSO.MapRegistry.AllMaps`
5. 给 `MapConnectionDatabaseSO` 新增 `PointScanCaches`
6. 修改 `MapDefinitionSO`，让 `MapId` 自动生成并只读显示
7. 修改 `MapLinkPoint`，将 `PointId` 改为自动生成的 `PointGuid`
8. 删除 `MapLinkPoint.ConnectionId`
9. 修改 `MapConnectionData`，删除 `ConnectionId`
10. 修改普通切图逻辑，改为 `CurrentMapId + PointGuid` 反查连接
11. 给 `MapConnectionDatabaseSO` 写自定义 Inspector
12. 在 Inspector 中实现 Registry 选择、地图下拉选择和点位下拉选择
13. 加连接表校验按钮
14. 加 `扫描全部点位` 按钮，用于扫描 `MapRegistrySO.AllMaps` 中所有地图的 `MapLinkPoint`
15. 用户在 Unity 中重新配置测试数据
16. 手动测试新游戏、普通切图、传送、存档读档

---

## 六、校验规则

连接表必须检查：

1. `MapRegistrySO` 是否为空
2. 端点保存的 `MapId` 是否能在 `MapRegistrySO.AllMaps` 中找到对应 `MapDefinitionSO`
3. `MapId` 是否重复
4. `PointGuid` 是否为空
5. 同一地图内 `PointGuid` 是否重复
6. 连接端点是否引用了不存在的点位
7. 一个端点是否出现在多条普通连接里
8. 连接是否缺少 `EndA` 或 `EndB`
9. `DisplayName` 是否为空
10. 连接端点引用的点位是否存在于 `PointScanCaches`
11. 扫描结果里是否存在空 `PointGuid` 的 `MapLinkPoint`

校验结果：

1. 用户手动点击 `校验` 按钮后，系统执行一次连接表合法性检查
2. 校验不通过：显示红色错误信息，提示用户修正
3. 普通警告：显示黄色提示，不影响运行
4. 校验通过：显示绿色或默认状态
5. `DisplayName` 为空只给警告，不影响运行
6. 扫描发现空 `PointGuid` 只给警告，不加入点位下拉

---

## 七、暂不做

### 7.1 不做复杂图形编辑器

原因：

1. 成本高
2. 容易变成另一个 `MapGraph`
3. 当前痛点是配置体验，不是缺图形化编辑

### 7.2 不做静默自动布线

原因：

1. 自动猜连接容易错
2. 错误连接很难排查
3. 更好的方式是扫描、提示、用户确认

### 7.3 不靠对象名匹配

原因：

1. GameObject 名字容易改
2. 改名会导致运行时断链
3. 运行时必须使用稳定 `PointGuid`

---

## 八、后续扩展

如果以后需要“同一个点根据剧情通往不同地图”，不要污染普通连接。

单独新增：

```text
ConditionalMapConnection
- FromEndpoint
- Targets
- Condition
- Priority
```

普通连接继续保持一对一，条件连接单独设计。

---

## 九、验收标准

完成后应满足：

1. 新建地图不需要手写 `MapId`
2. 新建点位不需要手写 `PointGuid`
3. 场景点位不再配置 `ConnectionId`
4. 普通连接数据不再保存 `ConnectionId`
5. 连接表不需要手写 `MapId / PointGuid`
6. 连接表能通过下拉选择点位
7. 普通切图能通过 `CurrentMapId + PointGuid` 找到唯一连接
8. 多连接匹配直接报错
9. 缺失连接直接报错
10. 连接表 Inspector 从 `MapRegistrySO.AllMaps` 获取地图清单
11. 连接表顶部只需要配置一次 `MapRegistrySO`
12. `EndA / EndB` 的地图选择来自 `MapRegistrySO.AllMaps` 下拉框
13. 连接表顶部只需要点击一次 `扫描全部点位`
14. `EndA / EndB` 的点位选择来自 `PointScanCaches` 下拉框
15. 点位没有 `DisplayName` 时，下拉显示 `未命名点 (PointGuid前8位)`，不能空白
