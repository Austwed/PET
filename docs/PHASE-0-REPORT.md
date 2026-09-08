# 阶段 0 可行性报告

日期：2026-09-08  
结论：**通过，可以进入阶段 1。**

## 已冻结的决策

| 项目 | 决策 |
| --- | --- |
| 目标平台 | Windows 10/11，当前验证环境为 Windows NT `10.0.26200.0` |
| 应用框架 | `.NET 10 + WPF` |
| 首版动画渲染 | WPF `BitmapSource` / `CroppedBitmap` 播放透明 PNG 图集；MVP 不引入 SkiaSharp |
| 主用量通道 | 本机 Codex `app-server` JSON-RPC |
| 降级通道 | `usage-snapshot.json`；再失败则显示“用量暂不可用” |
| 用量刷新 | 监听滚动更新，同时每 120 秒主动校准一次 |
| 角色代号 | `fengjin`；产品 ID 继续使用 `codex-usage-pet` |
| 默认显示尺寸 | 角色约 `240 × 260 DIP`，对应 1.25 倍基础单元格；允许 0.75–2.0 倍缩放 |
| 自动启动 | 默认关闭，由托盘设置显式开启 |
| 气泡语言与时长 | 简体中文，默认显示 8 秒，鼠标进入气泡时暂停消失计时 |

## 用量通道验证

本机环境：

- Codex CLI：`0.153.4`
- 可执行文件：由 `Get-Command codex` 动态解析，不在程序中硬编码版本目录。
- `codex app-server generate-json-schema --experimental` 成功生成协议定义。
- 独立进程通过 `codex app-server --stdio` 完成 JSON-RPC 初始化，并成功调用 `account/rateLimits/read`。
- 测试只读取限额，没有发送模型请求，没有读取或复制登录令牌、Cookie、配置凭据。

已验证的响应结构：

```text
rateLimitsByLimitId.codex
├─ primary
│  ├─ usedPercent
│  ├─ windowDurationMins = 300
│  └─ resetsAt
└─ secondary
   ├─ usedPercent
   ├─ windowDurationMins = 10080
   └─ resetsAt
```

桌宠按窗口时长识别 5 小时与每周窗口，不仅依赖 `primary` / `secondary` 字段名。剩余百分比计算为：

```text
remainingPercent = clamp(100 - usedPercent, 0, 100)
```

协议还包含 `account/rateLimits/updated` 稀疏滚动通知。客户端收到通知后合并非空值，并定期重新调用 `account/rateLimits/read` 进行完整校准。

### 稳定性判断

这条路线能工作，但 `app-server` 和其协议仍标记为实验性。因此实现必须：

- 在连接时记录并检查 CLI/协议版本。
- 对缺失字段、未知 `limitId` 和未来新增窗口宽容解析。
- 不保存响应中的 `accountId`，日志只记录脱敏后的窗口数据和错误类别。
- 启动失败或方法不存在时立即切换到快照通道，不尝试读取 Codex 凭据。
- 不依赖 CLI `/status` 的人类可读文本作为主数据源。

## 官方资料核对

官方 OpenAI 文档说明用户可在 Codex 设置/用量页查看限额、余额和重置时间，也可在活动 CLI 会话使用 `/status`。官方资料同时确认 5 小时和每周窗口都可能被重置，周重置日期可能因此改变，所以程序必须以每次响应中的 `resetsAt` 为准，不能按固定星期推算。

## .NET/WPF 环境验证

- 已安装 `.NET SDK 10.0.400`。
- 已安装 `Microsoft.WindowsDesktop.App 10.0.11` 运行时和目标包。
- 仅安装了 .NET 8 桌面运行时，没有 .NET 8 SDK/目标包，因此项目从原计划的 .NET 8 调整为 .NET 10。
- 沙箱中的 `dotnet new` 无权写用户级模板缓存；阶段 1 将使用项目内 CLI 缓存或手工创建最小 WPF 工程，再执行构建验证。

## 数据契约与测试数据

- `contracts/usage-snapshot.schema.json` 定义桌宠内部的稳定、脱敏数据格式。
- `fixtures/usage-threshold-cases.json` 覆盖单档、多档、双窗口、回升、周期重置与陈旧数据场景。
- `config/defaults.json` 固化首版交互时间、阈值、刷新与显示默认值。

## 视觉准备结果

视觉识别锚点、简化规则、色彩方向和动画约束已写入 `docs/VISUAL-BRIEF.md`。阶段 0 不生成主图；阶段 2 的第一项工作是基于该简报生成一张主参考图并让用户确认，确认前不批量生成动画。

## 阶段 1 入口条件

以下条件均已满足：

- [x] 本机可获取结构化的 5 小时与每周用量。
- [x] 已确定不读取浏览器 Cookie 或 Codex 凭据。
- [x] 已冻结 Windows 技术栈与无需第三方渲染依赖的 MVP 路线。
- [x] 已建立稳定数据契约、模拟场景与默认配置。
- [x] 已建立风堇视觉简报和 Codex v2 图集约束。

阶段 1 可以开始创建应用骨架、状态机、阈值引擎与占位图运行版。

