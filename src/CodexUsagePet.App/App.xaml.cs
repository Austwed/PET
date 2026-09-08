using System.Windows;
using CodexUsagePet.App.Infrastructure;
using CodexUsagePet.Core.Behavior;
using CodexUsagePet.Core.Usage;

namespace CodexUsagePet.App;

public partial class App : System.Windows.Application
{
    private SingleInstanceGuard? _singleInstance;
    private TrayIconService? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstance = new SingleInstanceGuard("CodexUsagePet.Fengjin.Singleton");
        if (!_singleInstance.IsPrimaryInstance)
        {
            System.Windows.MessageBox.Show("风堇桌宠已经在运行。", "Codex 用量桌宠");
            Shutdown();
            return;
        }

        var mockProvider = new MockUsageProvider();
        var window = new MainWindow(
            new PetStateMachine(PetTimingOptions.Default),
            new ThresholdEngine(),
            mockProvider);

        MainWindow = window;
        _trayIcon = new TrayIconService(
            summon: window.Summon,
            hide: window.Hide,
            refresh: () => window.RefreshUsageAsync(showBubble: true),
            simulateConsumption: window.SimulateConsumptionAsync,
            exit: Shutdown);

        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
