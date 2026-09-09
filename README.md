# Codex 剩余用量桌宠

一个面向 Windows 的透明悬浮桌宠：以《崩坏：星穹铁道》风堇的 Q 版同人形象为角色基础，显示 Codex 的 5 小时与每周剩余用量，并根据召唤、空闲、悬停、点击和用量阈值播放对应动画。

当前状态：**暂停；已保留可运行的真实风堇图集预览版，阶段 2 正式包仍停在最终 v2 图集验证故障。**

## 当前目标

- 制作一套符合 Codex `spriteVersionNumber: 2` 规范的原生宠物图集。
- 制作一个独立 Windows 桌面悬浮运行时，实现扩展互动、气泡和用量阈值逻辑。
- 支持直视、多个待机循环、无聊、期待、被摸头和哭泣等表现。
- 最终提供易启动的 Windows 可执行程序和 Codex 原生宠物包。

完整实施方案见 [docs/PLAN.md](docs/PLAN.md)。

## 运行当前预览版

需要 Windows 10/11 和 .NET 10 Desktop Runtime：

```powershell
dotnet restore CodexUsagePet.slnx --configfile NuGet.Config
dotnet build CodexUsagePet.slnx -c Release --no-restore
dotnet run --project src/CodexUsagePet.App/CodexUsagePet.App.csproj -c Release --no-build
```

当前版本加载 `assets/pets/fengjin/spritesheet-preview.png`：桌宠头部可点击，身体可拖动；托盘菜单可以召唤、隐藏、显示模拟用量、模拟消耗和退出。用量仍为模拟数据，图集仍有正式验证阻塞，适合本地查看进度而非正式发布。

运行行为测试：

```powershell
dotnet run --project tests/CodexUsagePet.Tests/CodexUsagePet.Tests.csproj -c Release --no-build
```

## 技术路线（暂定）

- 平台：Windows 10/11
- 运行时：.NET 10、WPF
- 图形与动画：首版使用 WPF `BitmapSource` / `CroppedBitmap`，透明无边框置顶窗口
- 配置：本地 JSON
- 用量数据：可替换的 `IUsageProvider`；首选本机 Codex `app-server` JSON-RPC，快照文件降级
- 发布：优先生成自包含的单文件 Windows `exe`

## 目录约定

```text
codex-usage-pet/
├─ README.md
├─ docs/                 # 计划、设计和验收文档
├─ config/               # 默认配置
├─ contracts/            # 稳定数据契约
├─ fixtures/             # 脱敏模拟与测试场景
├─ src/                  # 后续创建的程序源码
├─ assets/               # 项目专属角色与 UI 素材
├─ tests/                # 状态机和用量阈值测试
└─ tools/                # 项目专属构建与验证辅助工具
```

正式构建产物放到工作区根目录的 `08_Exports/codex-usage-pet`。

阶段报告见 [docs/PHASE-0-REPORT.md](docs/PHASE-0-REPORT.md)、[docs/PHASE-1-REPORT.md](docs/PHASE-1-REPORT.md) 和 [docs/PHASE-2-CHECKPOINT.md](docs/PHASE-2-CHECKPOINT.md)，角色约束见 [docs/VISUAL-BRIEF.md](docs/VISUAL-BRIEF.md)。

## 边界

- 这是个人使用的同人桌宠，不提取或转售游戏、华硕天选姬的原始资产。
- “仿照天选姬”只作为紧凑全身比例、桌面可读轮廓和互动节奏参考，不复制具体造型、动画帧或商标元素。
- 不通过抓取账号凭据、Cookie 或绕过访问控制来读取 Codex 用量。
