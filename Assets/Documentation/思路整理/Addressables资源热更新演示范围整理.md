# Addressables资源热更新演示范围整理

## 推荐做到

1. 远程资源组

用 Addressables Remote Group 放 1 到 3 类资源：角色、怪物 Prefab、UI 图标、配置 JSON 任选。

2. 远程 Catalog 更新

能跑通：

```text
CheckForCatalogUpdates
-> UpdateCatalogs
-> DownloadDependenciesAsync
-> LoadAssetAsync
```

3. 进度展示

有一个简单 UI 显示：

```text
检查更新
-> 下载大小
-> 下载进度
-> 完成或失败
```

4. 真实替换效果

本地先显示旧资源，上传新 bundle 后，重启或点按钮能看到资源变化。

5. 缓存与失败处理

至少处理：

```text
无更新
网络失败
下载失败
缓存命中
清理缓存重新下载
```

6. 一页流程说明

写清楚构建、上传、运行、验证步骤。

## 项目样式口径

基于 Addressables 搭建资源热更新闭环，支持远程 Catalog 检查、资源包下载、缓存复用、失败回退，并通过替换角色、怪物或 UI 资源验证运行时更新效果。
