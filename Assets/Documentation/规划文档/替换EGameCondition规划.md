在实现完整的任务系统之前 可以先吧EGameCondition替换的工作完成
DialogueNpcTriggerInfo & DialogueOneTriggerInfo
删掉eGameCondition
添加DialogueConditionSO[]字段
DialogueNpcTrigger 和 DialogueOneTrigger 中，原本调用 GameManager.CheckGameCondition 的地方，改为遍历
Conditions，全部 IsMet 返回 true 才通过

DialogueConditionSO
放在PlayArk/DialogueSystem/.../Data/Info/DialogueConditionSO.cs
这是个对话条件基类，继承ScriptableObject 作为条件资源存在
提供一个抽象方法：
    IsMet(); 用该方法自测当前条件是否满足

将EGameCondition里的条件，各自拆成对应的条件资源
放在Scripts/Dialogue/Conditions
SpiderWin -> SpiderBossAliveConditionSO
SpiderLose -> SpiderBossKilledConditionSO
FoxWin -> FoxBossAliveConditionSO
ClearMirrorMap -> MirrorMapClearedConditionSO
FoundChen -> FoundChenConditionSO
None -> AlwaysTrueConditionSO

不需要考虑旧资源迁移问题，项目中没有需要保留的旧资源

删除 GameManager.CheckGameCondition
删除 EGameCondition