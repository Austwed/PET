# 阶段 1 完成报告

日期：2026-09-09  
结论：**完成，可以进入阶段 2 主视觉制作。**

## 已实现

### WPF 桌宠骨架

- .NET 10 WPF 透明、无边框、置顶窗口。
- 任务栏隐藏、系统托盘菜单和双击召唤。
- 单实例互斥，避免重复启动多个桌宠。
- 角色身体区域可拖动，头部区域点击触发摸头。
- WPF 矢量占位角色，不包含任何游戏或品牌原始资产。
- 用量气泡显示两个窗口的模拟剩余量、重置倒计时和更新时间。

### 互动状态机

已实现并按优先级调度：

1. 用量阈值哭泣
2. 点击摸头
3. 鼠标悬停期待
4. 启动、召唤和近期互动直视
5. 长时间无聊
6. 安静、好奇、困倦三种轮换待机

高优先级动作结束后会重新计算当前状态。哭泣可以覆盖摸头，摸头可以覆盖悬停；悬停仍存在时，摸头结束后恢复期待。

### 用量与阈值核心

- 建立 `IUsageProvider`，阶段 1 使用 `MockUsageProvider`。
- 建立脱敏 `UsageSnapshot`、5 小时窗口和每周窗口模型。
- 剩余量统一从 `100 - usedPercent` 得出并限制在 `0–100`。
- 阈值引擎支持 `80/60/40/20/1` 向下穿越。
- 初始快照不触发；一次跨多档合并为单窗口事件；双窗口可以合并成一次 UI 反应。
- 重置时间变化会建立新周期且不会误触哭泣。
- 陈旧或不可用数据不会触发事件，也不会覆盖有效基线。

### 阶段 1 调试入口

托盘菜单包含：

- 召唤风堇
- 隐藏
- 显示用量
- 模拟消耗（每次让 5 小时剩余下降 7 点、每周下降 2 点）
- 退出

“模拟消耗”用于直观看到气泡和阈值哭泣，接入真实 Provider 后会从正式菜单移除。

## 工程结构

```text
src/
├─ CodexUsagePet.Core/   # 模型、状态机、用量接口和阈值引擎
└─ CodexUsagePet.App/    # WPF 窗口、占位视觉、托盘和单实例
tests/
└─ CodexUsagePet.Tests/  # 无第三方依赖的可执行测试集
```

项目没有第三方 NuGet 依赖，仓库内 `NuGet.Config` 清空外部包源，使现阶段可以离线还原和构建。

## 验证结果

```text
dotnet build CodexUsagePet.slnx -c Release
0 warnings, 0 errors

dotnet run --project tests/CodexUsagePet.Tests/CodexUsagePet.Tests.csproj -c Release --no-build
9/9 tests passed
```

另完成进程级启动冒烟测试：`CodexUsagePet.exe` 启动后保持运行 3 秒，无启动崩溃；测试实例随后按 PID 关闭。

本地发布目录位于综合工作区的 `08_Exports/codex-usage-pet/stage1/`，并另行生成 `CodexUsagePet-stage1-win.zip`；这些可再生成文件不提交进源码仓库。

## 当前限制

- 界面仍是矢量占位角色，没有正式风堇主视觉和逐帧动画。
- 用量来自模拟 Provider，真实 `app-server` Provider 安排在阶段 4。
- 默认配置文件已随构建复制，但设置 UI、动态读取和持久化尚未实现。
- 当前发布是依赖已安装 .NET 10 Desktop Runtime 的框架依赖版本；最终阶段再生成自包含单文件版本。
- 本阶段只做进程级 GUI 冒烟测试，正式角色加入后需做截图和多 DPI 视觉 QA。

## 下一阶段

阶段 2 将按 `docs/VISUAL-BRIEF.md` 生成并确认唯一主参考图，再完成 Codex v2 的 9 个标准动画行、四方向锚点、16 向视线、确定性验证与视觉 QA。
