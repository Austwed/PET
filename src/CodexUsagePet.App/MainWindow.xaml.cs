using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CodexUsagePet.Core.Behavior;
using CodexUsagePet.Core.Models;
using CodexUsagePet.Core.Usage;

namespace CodexUsagePet.App;

public partial class MainWindow : Window
{
    private readonly PetStateMachine _stateMachine;
    private readonly ThresholdEngine _thresholdEngine;
    private readonly MockUsageProvider _usageProvider;
    private readonly DispatcherTimer _timer;
    private UsageSnapshot? _latestSnapshot;
    private PetState _lastState = PetState.Direct;
    private DateTimeOffset _bubbleUntil;

    public MainWindow(
        PetStateMachine stateMachine,
        ThresholdEngine thresholdEngine,
        MockUsageProvider usageProvider)
    {
        InitializeComponent();
        _stateMachine = stateMachine;
        _thresholdEngine = thresholdEngine;
        _usageProvider = usageProvider;
        _stateMachine.Start(DateTimeOffset.Now);

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(150),
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    public void Summon()
    {
        if (!IsVisible)
        {
            Show();
        }

        Activate();
        Topmost = true;
        _stateMachine.Summon(DateTimeOffset.Now);
    }

    public async Task RefreshUsageAsync(bool showBubble)
    {
        _latestSnapshot = await _usageProvider.GetSnapshotAsync();
        var evaluation = _thresholdEngine.Evaluate(_latestSnapshot);
        if (evaluation.ShouldReact)
        {
            _stateMachine.UsageThresholdCrossed(DateTimeOffset.Now);
            showBubble = true;
        }

        if (showBubble)
        {
            ShowUsageBubble(_latestSnapshot);
        }
    }

    public async Task SimulateConsumptionAsync()
    {
        _usageProvider.SimulateConsumption();
        await RefreshUsageAsync(showBubble: true);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 24;
        Top = workArea.Bottom - Height - 24;
        await RefreshUsageAsync(showBubble: false);
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var now = DateTimeOffset.Now;
        var state = _stateMachine.GetState(now);
        if (state != _lastState)
        {
            if (state == PetState.Bored)
            {
                ShowUsageBubble(_latestSnapshot);
            }

            _lastState = state;
            ApplyPlaceholderVisual(state);
        }

        if (UsageBubble.Visibility == Visibility.Visible &&
            now >= _bubbleUntil &&
            !UsageBubble.IsMouseOver)
        {
            UsageBubble.Visibility = Visibility.Collapsed;
        }
    }

    private void PetSurface_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) =>
        _stateMachine.PointerEntered(DateTimeOffset.Now);

    private void PetSurface_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) =>
        _stateMachine.PointerExited(DateTimeOffset.Now);

    private async void HeadHitArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        _stateMachine.HeadPat(DateTimeOffset.Now);
        await RefreshUsageAsync(showBubble: true);
    }

    private void BodyDragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (InvalidOperationException)
        {
            // The button may be released before WPF begins the drag operation.
        }

        _stateMachine.PointerEntered(DateTimeOffset.Now);
    }

    private void ShowUsageBubble(UsageSnapshot? snapshot)
    {
        UsageText.Text = FormatUsage(snapshot);
        UsageBubble.Visibility = Visibility.Visible;
        _bubbleUntil = DateTimeOffset.Now.AddSeconds(8);
    }

    private static string FormatUsage(UsageSnapshot? snapshot)
    {
        if (snapshot is null || snapshot.Status != UsageAvailability.Available)
        {
            return "用量暂不可用\n稍后再来看看吧。";
        }

        var fiveHour = FormatWindow("5 小时", snapshot.FiveHour, snapshot.ObservedAt);
        var weekly = FormatWindow("每周", snapshot.Weekly, snapshot.ObservedAt);
        return $"{fiveHour}\n{weekly}\n更新于 {snapshot.ObservedAt:HH:mm:ss}";
    }

    private static string FormatWindow(
        string label,
        UsageWindowSnapshot? window,
        DateTimeOffset observedAt)
    {
        if (window is null)
        {
            return $"{label}：暂无数据";
        }

        var resetText = window.ResetsAt is null
            ? "重置时间未知"
            : $"约 {FormatRemaining(window.ResetsAt.Value - observedAt)} 后重置";
        return $"{label}剩余 {window.RemainingPercent}%（{resetText}）";
    }

    private static string FormatRemaining(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "即将";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"{(int)remaining.TotalDays} 天 {remaining.Hours} 小时";
        }

        return $"{Math.Max((int)remaining.TotalHours, 0)} 小时 {remaining.Minutes} 分";
    }

    private void ApplyPlaceholderVisual(PetState state)
    {
        (FaceGlyph.Text, StateText.Text) = state switch
        {
            PetState.Direct => ("◕‿◕", "直视"),
            PetState.IdleCalm => ("◡‿◡", "安静待机"),
            PetState.IdleCurious => ("◕‿◔", "好奇待机"),
            PetState.IdleSleepy => ("－‿－", "困倦待机"),
            PetState.Bored => ("¬_¬", "无聊"),
            PetState.Expecting => ("✧‿✧", "期待"),
            PetState.HeadPat => ("＞▽＜", "被摸头"),
            PetState.Crying => ("ಥ﹏ಥ", "用量提醒"),
            _ => ("◕‿◕", "直视"),
        };
    }
}
