# Cinemachine 摄像机系统重构方案

## 背景

当前每个场景都配置了一套完整的 Cinemachine 摄像机（CinemachineBrain + VCam_Follow），导致：
- 重复配置，参数不统一
- 修改相机行为需要改每个场景
- 新场景必须重新配一套

## 目标

Brain + 主 VCam 跨场景复用，场景中不再配置摄像机。

## 架构

```
┌──────────────────────────────────────────────────────┐
│  DontDestroyOnLoad（跨场景存活）                       │
│                                                      │
│  Main Camera                                         │
│  └─ CinemachineBrain                                 │
│                                                      │
│  VCam_Follow（主跟随相机，Priority=10）                │
│  ├─ Body: Framing Transposer                         │
│  ├─ Aim: Hard Lock To Target                         │
│  └─ Follow: Player（BindingPlayer 自动绑定）          │
│                                                      │
│  VCam_Dialogue（对话相机，Priority=20，默认禁用）      │
│  └─ Follow: 动态赋值（对话触发时绑定 NPC）             │
│                                                      │
│  CameraManager（单例）                                │
│  └─ 管理相机切换、对话镜头、Boss 战镜头                │
├──────────────────────────────────────────────────────┤
│  每个场景                                             │
│                                                      │
│  Boss 房间特殊相机点（已有，保持不变）                  │
└──────────────────────────────────────────────────────┘
```

## 实现步骤

### Step 1：创建 CameraManager

**新建文件**：`Assets/Scripts/Camera/CameraManager.cs`

```
职责：
- 单例，DontDestroyOnLoad
- 持有 VCam_Follow、VCam_Dialogue 引用
- 提供对话相机切换接口
- 提供 Boss 战相机控制接口

核心方法：
- SetupVCams() — 初始化相机引用
- EnterDialogue(Transform npcTransform) — 激活 VCam_Dialogue，Follow 绑定 NPC
- ExitDialogue() — 禁用 VCam_Dialogue，回到 VCam_Follow
- EnterBossFight(Vector3 bossCenter) — 断开 Follow，DOTween 移到 Boss 中心（复用现有 FoxBossRoom 逻辑）
- ExitBossFight() — 恢复 Follow Player
```

### Step 2：改造 BindingPlayer

**修改文件**：`Assets/Scripts/Camera/BindingPlayer.cs`

```
改动：
- 不再自己 FindGameObjectWithTag("Player")
- 改为从 CameraManager 获取 VCam 引用后绑定 Player
- 或者直接删除，绑定逻辑移入 CameraManager.SetupVCams()
```

### Step 3：改造对话系统相机切换

**修改文件**：
- `Assets/Scripts/Dialogue/DialogueNpcTrigger.cs`
- `Assets/Scripts/Dialogue/DialogueOneTrigger.cs`

```
改动：
- 删除 m_camera 字段
- 对话开始：CameraManager.Instance.EnterDialogue(npcTransform)
- 对话结束：CameraManager.Instance.ExitDialogue()
```

### Step 4：改造 Boss 房间相机逻辑

**修改文件**：
- `Assets/Scripts/Character/Monster/Fox/FoxBossRoom.cs`
- `Assets/Scripts/Character/Monster/Spider/SpiderBossRoom.cs`

```
改动：
- 删除各自场景中的独立 VCam 引用
- Boss 战开始：CameraManager.Instance.EnterBossFight(bossCenter)
- Boss 战结束：CameraManager.Instance.ExitBossFight()
- DOTween 移动逻辑移入 CameraManager
```

### Step 5：场景清理

每个场景中：
- 删除 Main Camera 上的 CinemachineBrain（由跨场景的 CameraManager 管理）
- 删除 VCam_Follow（由跨场景的 CameraManager 管理）
- 保留 Boss 战特殊相机点（仅作为位置标记，不挂 VCam）

### Step 6：初始场景配置

在起始场景（CampMap）中：
- Main Camera 挂 CinemachineBrain + CameraManager 脚本
- 创建 VCam_Follow（Priority=10）和 VCam_Dialogue（Priority=20，默认禁用）
- CameraManager 在 Awake 中 DontDestroyOnLoad 这些 GameObject

## 关键文件清单

| 文件 | 操作 |
|------|------|
| `Assets/Scripts/Camera/CameraManager.cs` | 新建 |
| `Assets/Scripts/Camera/BindingPlayer.cs` | 改造或删除 |
| `Assets/Scripts/Dialogue/DialogueNpcTrigger.cs` | 改造 |
| `Assets/Scripts/Dialogue/DialogueOneTrigger.cs` | 改造 |
| `Assets/Scripts/Character/Monster/Fox/FoxBossRoom.cs` | 改造 |
| `Assets/Scripts/Character/Monster/Spider/SpiderBossRoom.cs` | 改造 |
| 各场景 .unity 文件 | 删除重复相机 |

## VCam_Follow 参数配置（Framing Transposer）

| 参数 | 值 | 说明 |
|------|-----|------|
| Tracked Object Offset | X=0.5, Y=0.2 | 角色偏左下，前方有更多视野 |
| Lookahead Time | 0.3 | 移动预判 |
| Lookahead Smoothing | 0.75 | 预判平滑 |
| X Damping | 1.0 | 横向跟随速度 |
| Y Damping | 0.5 | 纵向跟随更快（跳跃灵敏） |
| X Dead Zone | 0.1 | 横向小死角 |
| Y Dead Zone | 0.05 | 纵向更小死角 |
| X Soft Zone | 0.3 | |
| Y Soft Zone | 0.2 | |

## 验证

1. 在 CampMap 启动，确认相机跟随 Player
2. 切换到其他场景，确认相机继续正常跟随
3. 触发对话，确认镜头切换到 NPC
4. 结束对话，确认镜头回到 Player
5. Boss 战，确认相机移到 Boss 中心
6. Boss 战结束，确认相机恢复跟随
7. 跳跃时确认 Y 轴跟随
8. Unity Console 无 Error
