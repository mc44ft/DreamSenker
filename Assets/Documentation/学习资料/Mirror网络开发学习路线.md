# Mirror 网络开发学习路线

## 结论

Mirror 是可以直接拿来做 Unity 联机游戏的网络框架。

学习目标不是先啃完整源码，而是：

```text
先会用 Mirror 做出联机闭环；
再理解背后的网络模型；
最后带着问题读局部源码。
```

更适合当前阶段的节奏：

```text
70% 写小 Demo
20% 读官方文档和局部源码
10% 看课程或视频破冰
```

## 不建议一上来啃 Mirror 全源码

Mirror 源码会涉及：

```text
Transport
NetworkManager
NetworkIdentity
NetworkBehaviour
NetworkServer
NetworkClient
RPC
SyncVar
序列化
Spawn 系统
Authority
Interest Management
Weaver
```

如果没有先做过 Demo，直接读源码很容易变成：

```text
每个类好像能看懂，但整体为什么这样设计看不懂。
```

所以源码不是主线。

主线应该是：

```text
框架使用方式
网络同步模型
服务端权威设计
小型联机 Demo
局部源码验证
```

## 第一阶段：先学联机基础概念

先把概念搞清楚，不要一上来写代码。

必须理解：

```text
Client
Server
Host
Dedicated Server
Listen Server
Authoritative Server
RPC
State Sync
Input Sync
Snapshot
Tick
Interpolation
Client Prediction
Server Reconciliation
Lag Compensation
Interest Management
```

面试重点：

```text
联机游戏的核心不是“把对象同步过去”。
核心是决定谁有权修改状态、状态怎么同步、延迟怎么掩盖、作弊怎么防。
```

### 基础概念说明

#### Client

客户端。

玩家运行的游戏程序，负责：

```text
读取输入
显示画面
播放音效
本地 UI
本地表现
```

客户端可以发请求，但不应该直接决定重要结果。

#### Server

服务器。

负责接收客户端请求、计算权威状态、把结果同步给客户端。

服务器通常负责：

```text
伤害判定
血量变化
物品生成
任务状态
死亡重生
关键场景状态
```

#### Host

主机模式。

一个玩家同时是：

```text
Client
Server
```

优点是简单、省服务器成本。

缺点是房主有优势，房主退出会影响房间。

#### Dedicated Server

专用服务器。

只运行服务器逻辑，不参与游戏，不渲染画面。

适合正式联机游戏。

特点：

```text
稳定
公平
成本更高
部署复杂
```

#### Listen Server

监听服务器。

和 Host 很像：玩家本机开服务器，同时自己也玩。

优点是简单。

缺点是：

```text
房主网络影响所有人
房主可能有优势
房主退出房间容易中断
```

#### Authoritative Server

权威服务器。

核心原则：

```text
重要状态由服务器说了算。
```

例如：

```text
客户端不能说“我打中了，所以敌人死了”
客户端只能说“我在这个时间点尝试攻击”
服务器判断是否打中
服务器同步最终结果
```

这是防作弊和保证一致性的基础。

#### RPC

远程过程调用。

简单说：

```text
让远端执行一个函数。
```

常见方向：

```text
客户端 -> 服务端：请求攻击、请求拾取、请求交互
服务端 -> 客户端：播放特效、显示提示、同步一次性事件
```

Mirror 里常见：

```text
Command
ClientRpc
TargetRpc
```

#### State Sync

状态同步。

同步的是“现在是什么状态”。

例子：

```text
血量 = 80
门 = 已打开
怪物位置 = (10, 2)
任务状态 = 已完成
拾取物 = 已消失
```

Mirror 对应：

```text
SyncVar
SyncList
NetworkTransform
```

#### Input Sync

输入同步。

客户端不直接同步结果，只上传输入。

例子：

```text
第 100 tick：
方向键 = 右
跳跃键 = 按下
攻击键 = 按下
```

服务器拿输入去模拟或校验结果。

动作游戏里，输入同步通常比直接相信客户端结果更安全。

#### Snapshot

快照。

某一个网络 tick 下的世界状态合集。

例子：

```text
Tick 120:
玩家A位置
玩家B位置
敌人血量
门状态
子弹列表
拾取物状态
```

服务器把快照发给客户端，客户端根据快照更新本地表现。

#### Tick

网络逻辑帧。

服务器按固定频率处理网络世界。

例子：

```text
30 tick/s = 每 33.3ms 处理一次
60 tick/s = 每 16.6ms 处理一次
```

tick 越高，响应越细，但带宽和 CPU 压力也更大。

#### Interpolation

插值。

网络状态是一包一包来的，不是每帧都有。

插值就是在两个已知状态之间平滑过渡。

例子：

```text
上一包位置 x = 0
下一包位置 x = 10
客户端不要瞬移到 10
而是在 0 到 10 之间平滑移动
```

远端玩家和怪物常用插值。

#### Client Prediction

客户端预测。

客户端不等服务器确认，先根据自己的输入本地模拟。

例子：

```text
玩家按右
本地角色立刻往右走
同时把输入发给服务器
```

否则每次移动都等服务器确认，操作会非常粘手。

#### Server Reconciliation

服务端校正。

客户端预测可能和服务器权威结果不一致。

服务器返回权威状态后，客户端需要修正。

例子：

```text
客户端预测自己在 x = 10
服务器说实际在 x = 8
客户端回到 x = 8
再重放后续输入
```

预测和校正通常成对出现。

#### Lag Compensation

延迟补偿。

服务器做命中判定时，考虑玩家网络延迟，回看过去某个时间点的状态。

例子：

```text
你开枪时看到敌人在 A 点
服务器收到请求时敌人已经跑到 B 点
服务器按你开枪那一刻看到的历史位置判断
```

常用于射击游戏。

#### Interest Management

兴趣管理。

不是所有客户端都需要知道所有对象。

例子：

```text
玩家在地图左边
不需要同步地图右边 100 个怪物的详细状态
```

作用：

```text
减少带宽
减少同步对象数量
减少客户端处理压力
```

### 概念串联

一个典型权威服务器游戏可以这样理解：

```text
Client 上传 Input
Server 按 Tick 处理输入
Server 修改权威状态
Server 生成 Snapshot
Client 收到 State Sync
Client 用 Interpolation 平滑远端对象
本地玩家用 Client Prediction 提前响应
预测错了用 Server Reconciliation 校正
命中判定用 Lag Compensation 减少延迟不公平
Interest Management 控制哪些状态发给谁
```

## 第二阶段：先会用 Mirror

先按官方文档和简单 Demo 学会这些：

```text
NetworkManager
NetworkIdentity
NetworkBehaviour
Command
ClientRpc
TargetRpc
SyncVar
NetworkTransform
Server Authority
Client Authority
NetworkServer.Spawn
NetworkServer.Destroy
Scene Sync
```

对应要能回答：

```text
Command 是谁调用谁执行？
ClientRpc 是谁调用谁执行？
TargetRpc 和 ClientRpc 区别是什么？
SyncVar 和 RPC 区别是什么？
NetworkIdentity 的作用是什么？
NetworkBehaviour 和普通 MonoBehaviour 区别是什么？
Server Authority 为什么重要？
```

## 第三阶段：做一个最小联机 Demo

不要直接改 DreamSenker。

先单独做一个极小 Demo：

```text
2D 双人房间
玩家进入房间
玩家移动同步
玩家攻击
服务端判定伤害
血量 SyncVar 同步
死亡后服务端重生
拾取物服务端 Spawn / Despawn
一个开关状态同步
```

这个 Demo 的目标不是好玩，而是打通联机闭环。

验收标准：

```text
客户端不能自己改血量
客户端不能自己生成拾取物
服务端决定伤害结果
客户端只发输入或请求
重要状态都能被后来加入的客户端正确看到
断开重连后状态不乱
```

## 第四阶段：理解同步策略

做完 Demo 后，再开始分问题理解同步策略。

### 状态同步

适合长期存在的状态：

```text
血量
位置
当前动画状态
门是否打开
任务是否完成
拾取物是否存在
```

Mirror 对应工具：

```text
SyncVar
SyncList
NetworkTransform
服务端 Spawn / Destroy
```

### 事件同步

适合发生一次的行为：

```text
播放攻击特效
播放受击音效
播放死亡动画
弹出提示
```

Mirror 对应工具：

```text
ClientRpc
TargetRpc
```

### 输入同步

适合动作游戏：

```text
客户端上传输入
服务端模拟或校验
服务端广播权威结果
客户端做预测和修正
```

重点理解：

```text
同步状态不是越多越好。
能同步输入就不要同步一堆结果；
能服务端决定就不要相信客户端。
```

## 第五阶段：读局部源码

只读和问题直接相关的入口。

推荐顺序：

```text
NetworkBehaviour
NetworkIdentity
NetworkManager
NetworkServer
NetworkClient
Transport 抽象
SyncVar 相关文档和 Weaver 生成逻辑
```

读源码时只追这些问题：

```text
Command 怎么从客户端发送到服务端？
ClientRpc 怎么从服务端广播到客户端？
SyncVar 为什么能自动同步？
NetworkIdentity 如何标识一个网络对象？
NetworkServer.Spawn 做了哪些关键事情？
Authority 是怎么判断的？
```

不要做这种事：

```text
从第一个文件开始逐行读完整源码。
```

这很慢，收益也低。

## 第六阶段：对照 DreamSenker 思考改造边界

不要一口气把项目改成联机。

先拆系统：

```text
玩家输入
玩家移动
攻击判定
血量
敌人 AI
拾取物
地图切换
任务系统
背包系统
UI
存档
```

逐个判断：

```text
谁拥有状态？
谁能修改状态？
哪些状态要同步？
哪些行为只需要广播事件？
哪些逻辑必须服务端判定？
哪些 UI 只在本地显示？
```

当前项目如果做联机，优先从小范围开始：

```text
双人同地图
只同步玩家移动、攻击、血量、拾取物
暂时不碰完整任务系统和存档系统
```

## 面试准备重点

优先准备这些问题：

```text
TCP 和 UDP 区别
可靠传输和不可靠传输
C/S 架构
Host 和 Dedicated Server 区别
Server Authority
RPC 和状态同步区别
帧同步和状态同步区别
客户端预测
服务端回滚/校正
插值和平滑
延迟补偿
断线重连
心跳和超时
同步频率
带宽优化
防作弊思路
```

Mirror 相关要能说：

```text
NetworkBehaviour 是网络版 MonoBehaviour。
NetworkIdentity 标识网络对象。
Command 是客户端请求服务端执行。
ClientRpc 是服务端通知客户端执行。
TargetRpc 是服务端通知指定客户端执行。
SyncVar 是服务端状态自动同步到客户端。
重要状态应该由服务端修改，客户端不要直接决定结果。
```

## 推荐学习资料

Mirror 官方：

- Mirror GitHub  
  https://github.com/MirrorNetworking/Mirror
- Mirror 文档  
  https://mirror-networking.gitbook.io/docs

Unity 官方参考：

- Unity Netcode for GameObjects 文档  
  https://docs.unity.com/en-us/multiplayer/netcode/netcode

其他框架参考：

- FishNet GitHub  
  https://github.com/FirstGearGames/FishNet
- Photon Fusion 文档  
  https://doc.photonengine.com/fusion/current/getting-started/fusion-intro

## 最小学习闭环

按这个顺序做：

```text
1. 看 Mirror 官方 Getting Started
2. 跑通一个官方或社区 Demo
3. 自己做 2D 双人移动同步
4. 加入服务端权威血量
5. 加入攻击 RPC
6. 加入拾取物 Spawn / Despawn
7. 加入死亡和重生
8. 回头读 NetworkBehaviour / NetworkIdentity / SyncVar / Command 局部源码
9. 总结一份“单机系统改联机系统”的规则
```

最终目标：

```text
不是“读懂 Mirror 源码”。
而是能解释一个单机功能如何拆成客户端输入、服务端判定、状态同步和客户端表现。
```
