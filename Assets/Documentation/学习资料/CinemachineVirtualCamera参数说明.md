# CinemachineVirtualCamera 参数说明

## 结论

当前项目使用的是 Cinemachine 2.10.5。

2D 平台跳跃相机主要看这几块：

```text
CinemachineVirtualCamera
-> Follow / LookAt
-> Lens
-> Body: Framing Transposer
-> Aim
-> Extensions
```

你现在遇到的“跳一下镜头轻微上下动、回来的位置不一致”，优先看 `Framing Transposer`：

```text
Lookahead Time
Screen Y
Y Damping
Dead Zone Height
Soft Zone Height
Tracked Object Offset
```

## Virtual Camera 基础参数

### Follow

相机跟随谁。

2D 平台游戏里通常绑定玩家 Transform。`Framing Transposer` 会根据 Follow 目标的位置移动虚拟相机。

### Look At

相机看向谁。

2D 正交相机通常可以不填。对话镜头、Boss 镜头、3D 镜头才更常用。

### Priority

虚拟相机优先级。

多个虚拟相机同时启用时，CinemachineBrain 会选择 Priority 更高的相机。比如：

```text
Follow Camera Priority = 10
Dialogue Camera Priority = 11
```

对话相机启用后会抢过 Follow 相机。

### Standby Update

虚拟相机没被主相机采用时，是否继续更新。

常见值：

```text
Never      不更新，省性能
Always     一直更新，切过去时状态更准
RoundRobin 分批更新，折中
```

普通项目保持默认即可。

## Lens 参数

### Orthographic Size

正交相机视野大小。

2D 项目最关键的视野参数。值越大，看得越远，角色越小；值越小，看得越近，角色越大。

当前项目已经按地图配置这个值：

```text
FollowCameraOrthoSize
FollowCameraBossOrthoSize
DialogueCameraOrthoSize
```

### Field Of View

透视相机视野角度。

正交相机基本不用管。

### Near Clip Plane / Far Clip Plane

相机能看见的最近和最远距离。

2D 项目一般保持默认即可，只要角色和地图在裁剪范围内就行。

### Dutch

画面旋转角度。

常用于倾斜镜头。2D 平台日常相机一般保持 `0`。

### Lens Shift

镜头画面偏移。

它会偏移最终画面，不等于移动相机。2D 平台日常相机一般不用它，优先用 `Screen Y` 或 `Tracked Object Offset`。

## Body: Framing Transposer

`Framing Transposer` 是 2D 跟随相机最常用的 Body。它的核心职责是：

```text
让 Follow 目标保持在屏幕指定区域
```

### Tracked Object Offset

跟随目标的偏移点。

如果 Follow 绑定玩家 Transform，但玩家 Transform 在脚底，可以设置：

```text
Tracked Object Offset Y = 1 或 1.5
```

这样相机实际跟的是玩家身体上方，而不是脚底。

### Lookahead Time

提前预测目标未来位置。

值越大，相机会越倾向于朝玩家运动方向提前偏移。它适合横向奔跑预判，但很容易放大跳跃动画、刚体速度、落地抖动。

平台跳跃里如果镜头上下飘，优先设为：

```text
Lookahead Time = 0
```

### Lookahead Smoothing

预测位置的平滑程度。

Lookahead Time 为 0 时，这个值基本无意义。

### Lookahead Ignore Y

是否忽略 Y 方向预测。

如果想保留横向预判，但不想跳跃影响相机，可以打开它。

推荐优先方案：

```text
Lookahead Time = 0
```

如果确实需要横向预判，再试：

```text
Lookahead Time > 0
Lookahead Ignore Y = true
```

### X Damping / Y Damping / Z Damping

相机跟随阻尼。

值越大，相机越慢、越稳；值越小，相机越贴目标。

2D 平台相机里：

```text
X Damping 控制横向跟随手感
Y Damping 控制跳跃时镜头是否上下追
Z Damping 2D 正交相机通常影响不大
```

如果跳一下镜头就上下动，通常要提高 `Y Damping`，或者增大 `Dead Zone Height`。

### Target Movement Only

只根据目标自身移动计算阻尼。

通常保持开启。它可以减少因为相机自身旋转或其他变化带来的额外影响。

### Screen X / Screen Y

目标在屏幕中的理想位置。

范围可以理解成：

```text
0   屏幕左/下
0.5 屏幕中心
1   屏幕右/上
```

`Screen Y = 0.5` 表示玩家理想位置在屏幕正中。平台跳跃常用：

```text
Screen Y = 0.35 ~ 0.45
```

这样玩家偏下，屏幕上方留更多跳跃视野。

### Camera Distance

相机和目标的 Z 轴距离。

2D 正交相机里通常不是调视野的主参数。视野大小优先调 `Orthographic Size`。

### Dead Zone Width / Dead Zone Height

死区大小。

目标在死区内移动时，相机不动。

平台跳跃里，`Dead Zone Height` 决定“一段跳镜头会不会动”：

```text
Dead Zone Height 大
-> 玩家小幅跳跃时相机不动

Dead Zone Height 小
-> 玩家一跳相机就追
```

### Dead Zone Depth

Z 方向死区。

2D 正交平台相机基本不用管。

### Unlimited Soft Zone

软区是否无限大。

打开后相机更容易一直用阻尼慢慢追目标，调参直觉会变差。平台跳跃日常相机建议先关闭。

### Soft Zone Width / Soft Zone Height

软区大小。

目标离开 Dead Zone 后，进入 Soft Zone 时，相机会用 Damping 把目标慢慢带回 Dead Zone。

关系是：

```text
Dead Zone
-> 完全不动

Soft Zone
-> 慢慢修正

Soft Zone 外
-> 更强制地修正
```

如果 Soft Zone 太小，镜头会显得敏感、边界感强。

### Bias X / Bias Y

软区偏置。

它会偏移软区中心。一般先保持 `0`，别用它解决普通构图问题。

### Center On Activate

虚拟相机启用时是否尝试把目标居中到配置区域。

切换镜头或初次激活相机时有用。但如果目标刚生成、Follow 刚绑定，仍可能需要代码里调用 `OnTargetObjectWarped` / `CancelDamping` 刷新缓存。

## Group Framing 参数

这些参数用于跟随目标组，比如多角色同屏。

单玩家 2D 平台相机一般不用重点调。

### Group Framing Mode

目标组构图模式。

单 Follow 目标时影响不大。

### Adjustment Mode

目标组超出画面时，相机如何调整。

可能通过移动相机、改变 FOV、改变正交大小来适配目标组。单玩家一般不用。

### Group Framing Size

目标组占屏幕比例。

单玩家一般不用。

### Max Dolly In / Max Dolly Out

相机为了容纳目标组，最多能前后移动多少。

2D 正交单玩家基本不用。

### Minimum Distance / Maximum Distance

相机距离限制。

2D 正交单玩家基本不用。

### Minimum FOV / Maximum FOV

透视相机 FOV 限制。

正交相机基本不用。

### Minimum Ortho Size / Maximum Ortho Size

正交视野大小限制。

只有启用了自动调整正交大小时才重要。当前项目主要由地图配置直接设置 `Orthographic Size`。

## Aim 参数

Aim 决定相机朝向。

2D 正交平台相机通常让相机固定朝前，不需要复杂 Aim。常见选择：

```text
Do Nothing
Composer
```

如果使用 Composer，`Screen X/Y`、Dead Zone、Soft Zone 会影响相机旋转，而不是移动。2D 平台跟随一般优先用 Body 的 `Framing Transposer` 控制移动。

## Extensions

Extensions 是虚拟相机扩展组件。

常见用途：

```text
Cinemachine Confiner  限制相机不要超出地图边界
Cinemachine Impulse   接收镜头震动
```

如果相机“回不到同一位置”，除了 Framing Transposer，也要检查 Confiner 是否把相机推回了地图边界。

## 当前 Follow Camera 重点

当前 Follow Camera 里能看到这些值：

```text
Tracked Object Offset Y = 1.5
Lookahead Time = 0.261
Lookahead Ignore Y = false
Y Damping = 1
Screen Y = 0.5
Dead Zone Height = 1.15
Soft Zone Height = 1.15
```

其中最可疑的是：

```text
Lookahead Time = 0.261
Lookahead Ignore Y = false
```

这会让跳跃 Y 方向也参与预测，容易造成镜头轻微上下浮动。

## 平台跳跃推荐起步值

如果目标是“一段跳镜头不动，二段跳或高跳才轻微上顶”，可以先试：

```text
Lookahead Time = 0
Lookahead Smoothing = 0
Lookahead Ignore Y = false

Screen Y = 0.40
Y Damping = 2 ~ 3
Dead Zone Height = 0.35 ~ 0.55
Soft Zone Height = 0.8 ~ 1.0
Tracked Object Offset Y = 按玩家锚点微调
```

调参顺序：

```text
1. 先关 Lookahead
2. 调 Screen Y，确定玩家站立构图
3. 调 Dead Zone Height，让一段跳不推相机
4. 调 Soft Zone Height，让二段跳推相机时不突兀
5. 调 Y Damping，让上移和回落速度稳定
6. 最后检查 Confiner 有没有把相机推开
```

不要一开始写死相机高度。那会和地图出生点、正交视野、Confiner 边界互相打架。
