# CLAUDE.md

先读取并遵守：

- @AGENTS.md

本文件只作为 Claude Code 入口。项目规则以 AGENTS.md 为准。

关于项目技能：入口在 .claude/skills/，实现细节在 .agents/skills/。
请在用户说出相关指令时调用对应skill

写完代码后不要立刻结束。必须做自检：

  1. 先看改了哪些文件
     git diff --stat
     git diff --name-only

  2. 查关键符号引用
     用 rg 查新增/修改的类名、方法名、枚举名、字段名。
     确认没有旧名字残留、没有漏改调用点。
     例如：
     rg -n "OldManCanSeePlayer|CanSeeTarget|DamageableHealth|Health" Assets

  3. 查语法级明显错误
     不要靠猜。看 using、namespace、类名、接口方法签名是否匹配。
     Unity 项目里不要默认 dotnet build，除非用户要求。

  4. 查 diff 质量
     git diff --check
     发现尾随空格、冲突标记、坏格式就修。

  5. 看 Unity Console Error
     用 UnityMCP console_get_logs，只看 Error。
     有 Error 必须定位到文件和行，不允许带着红错结束。

  6. 自己读一遍本次改动
     重点看：
     - 生命周期顺序：Awake / Start / Update 有没有错
     - 序列化字段有没有破坏 prefab / asset
     - null 分支有没有处理
     - 接口实现有没有漏
     - enum 改名有没有让旧 asset 数值语义变掉
     - Unity 资产和 .meta 有没有被无意修改

  7. 最后汇报
     说清楚：
     - 改了什么
     - 检查了什么
     - 还有什么没验证

  DreamSenker 这个项目我默认的轻量验证是：

  rg -n "关键符号" Assets
  git diff --check

  再加 Unity Console：

  console_get_logs(logTypeFilter: "Error")

  重点是：不要只写完代码就说完成。Unity 项目很多错误不是 IDE 当场爆，是 Console、序列化、Prefab 引用、枚举
  资产值一起爆。