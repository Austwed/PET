using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace CodexUsagePet.App.Infrastructure;

public sealed class TrayIconService : IDisposable
{
    private readonly Forms.NotifyIcon _notifyIcon;
    private readonly Forms.ContextMenuStrip _menu;

    public TrayIconService(
        Action summon,
        Action hide,
        Func<Task> refresh,
        Func<Task> simulateConsumption,
        Action exit)
    {
        _menu = new Forms.ContextMenuStrip();
        _menu.Items.Add("召唤风堇", null, (_, _) => summon());
        _menu.Items.Add("隐藏", null, (_, _) => hide());
        _menu.Items.Add("显示用量", null, async (_, _) => await refresh());
        _menu.Items.Add("模拟消耗（阶段 1）", null, async (_, _) => await simulateConsumption());
        _menu.Items.Add(new Forms.ToolStripSeparator());
        _menu.Items.Add("退出", null, (_, _) => exit());

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Codex 用量桌宠（阶段 1）",
            Icon = Drawing.SystemIcons.Information,
            ContextMenuStrip = _menu,
            Visible = true,
        };
        _notifyIcon.DoubleClick += (_, _) => summon();
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
    }
}

