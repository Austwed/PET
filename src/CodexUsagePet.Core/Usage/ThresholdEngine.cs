using CodexUsagePet.Core.Models;

namespace CodexUsagePet.Core.Usage;

public sealed record ThresholdEvent(
    UsageWindowKind Window,
    int LowestCrossedThreshold,
    IReadOnlyList<int> CrossedThresholds,
    int RemainingPercent);

public sealed record ThresholdEvaluation(IReadOnlyList<ThresholdEvent> Events)
{
    public bool ShouldReact => Events.Count > 0;

    public static ThresholdEvaluation None { get; } = new([]);
}

public sealed class ThresholdEngine
{
    public static readonly IReadOnlyList<int> DefaultThresholds = [80, 60, 40, 20, 1];

    private readonly int[] _thresholds;
    private readonly Dictionary<UsageWindowKind, WindowState> _states = [];

    public ThresholdEngine(IEnumerable<int>? thresholds = null)
    {
        _thresholds = (thresholds ?? DefaultThresholds)
            .Distinct()
            .Where(value => value is >= 0 and <= 100)
            .OrderByDescending(value => value)
            .ToArray();

        if (_thresholds.Length == 0)
        {
            throw new ArgumentException("At least one threshold is required.", nameof(thresholds));
        }
    }

    public ThresholdEvaluation Evaluate(UsageSnapshot snapshot)
    {
        if (snapshot.Status != UsageAvailability.Available)
        {
            return ThresholdEvaluation.None;
        }

        var events = new List<ThresholdEvent>(2);
        EvaluateWindow(UsageWindowKind.FiveHour, snapshot.FiveHour, events);
        EvaluateWindow(UsageWindowKind.Weekly, snapshot.Weekly, events);
        return events.Count == 0 ? ThresholdEvaluation.None : new ThresholdEvaluation(events);
    }

    private void EvaluateWindow(
        UsageWindowKind kind,
        UsageWindowSnapshot? current,
        ICollection<ThresholdEvent> events)
    {
        if (current is null)
        {
            return;
        }

        if (!_states.TryGetValue(kind, out var previous))
        {
            _states[kind] = WindowState.CreateBaseline(current);
            return;
        }

        if (HasPeriodChanged(previous.ResetsAt, current.ResetsAt))
        {
            _states[kind] = WindowState.CreateBaseline(current);
            return;
        }

        var crossed = _thresholds
            .Where(threshold =>
                previous.RemainingPercent > threshold &&
                current.RemainingPercent <= threshold &&
                !previous.NotifiedThresholds.Contains(threshold))
            .ToArray();

        foreach (var threshold in crossed)
        {
            previous.NotifiedThresholds.Add(threshold);
        }

        previous.RemainingPercent = current.RemainingPercent;
        previous.ResetsAt = current.ResetsAt;

        if (crossed.Length > 0)
        {
            events.Add(new ThresholdEvent(
                kind,
                crossed.Min(),
                crossed,
                current.RemainingPercent));
        }
    }

    private static bool HasPeriodChanged(DateTimeOffset? previous, DateTimeOffset? current) =>
        previous.HasValue && current.HasValue && previous.Value != current.Value;

    private sealed class WindowState
    {
        private WindowState(int remainingPercent, DateTimeOffset? resetsAt)
        {
            RemainingPercent = remainingPercent;
            ResetsAt = resetsAt;
        }

        public int RemainingPercent { get; set; }

        public DateTimeOffset? ResetsAt { get; set; }

        public HashSet<int> NotifiedThresholds { get; } = [];

        public static WindowState CreateBaseline(UsageWindowSnapshot snapshot) =>
            new(snapshot.RemainingPercent, snapshot.ResetsAt);
    }
}

