---
name: package-search
description: "Search for packages in both Unity Package Manager registry and installed packages. Use this to find packages by name before installing them. Returns available versions and installation status. Searches both the Unity registry and locally installed packages (including Git, local, and embedded sources). Results are prioritized: exact name match, exact display name match, name substring, display name substring, description substring. Note: Online mode fetches exact matches from live registry, then supplements with cached substring matches."
---

# Package Search

先读取并遵守：

- @.agents/skills/package-search/SKILL.md

本文件只作为 Claude Code 技能入口。实际规则以 .agents/skills/package-search/SKILL.md 为准。
