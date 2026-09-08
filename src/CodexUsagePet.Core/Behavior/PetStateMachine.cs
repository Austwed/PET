namespace CodexUsagePet.Core.Behavior;

public enum PetState
{
    Direct,
    IdleCalm,
    IdleCurious,
    IdleSleepy,
    Bored,
    Expecting,
    HeadPat,
    Crying,
}

public sealed record PetTimingOptions(
    TimeSpan SummonDirect,
    TimeSpan RecentInteractionDirect,
    TimeSpan IdleAfter,
    TimeSpan BoredAfter,
    TimeSpan IdleVariantDuration,
    TimeSpan HeadPatDuration,
    TimeSpan CryingDuration)
{
    public static PetTimingOptions Default { get; } = new(
        SummonDirect: TimeSpan.FromSeconds(8),
        RecentInteractionDirect: TimeSpan.FromSeconds(12),
        IdleAfter: TimeSpan.FromSeconds(20),
        BoredAfter: TimeSpan.FromMinutes(10),
        IdleVariantDuration: TimeSpan.FromSeconds(8),
        HeadPatDuration: TimeSpan.FromSeconds(2.2),
        CryingDuration: TimeSpan.FromSeconds(4));
}

public sealed class PetStateMachine
{
    private readonly PetTimingOptions _options;
    private DateTimeOffset _lastInteractionAt;
    private DateTimeOffset _directUntil;
    private DateTimeOffset _reactionUntil;
    private PetState? _reaction;
    private int _reactionPriority;
    private bool _pointerOver;
    private bool _started;

    public PetStateMachine(PetTimingOptions? options = null)
    {
        _options = options ?? PetTimingOptions.Default;
    }

    public void Start(DateTimeOffset now)
    {
        _started = true;
        _lastInteractionAt = now;
        _directUntil = now + _options.SummonDirect;
    }

    public void Summon(DateTimeOffset now)
    {
        EnsureStarted(now);
        _lastInteractionAt = now;
        _directUntil = now + _options.SummonDirect;
    }

    public void PointerEntered(DateTimeOffset now)
    {
        EnsureStarted(now);
        _pointerOver = true;
        _lastInteractionAt = now;
    }

    public void PointerExited(DateTimeOffset now)
    {
        EnsureStarted(now);
        _pointerOver = false;
        _lastInteractionAt = now;
        _directUntil = now + _options.RecentInteractionDirect;
    }

    public void HeadPat(DateTimeOffset now)
    {
        EnsureStarted(now);
        TriggerReaction(PetState.HeadPat, priority: 1, now, _options.HeadPatDuration);
    }

    public void UsageThresholdCrossed(DateTimeOffset now)
    {
        EnsureStarted(now);
        TriggerReaction(PetState.Crying, priority: 2, now, _options.CryingDuration);
    }

    public PetState GetState(DateTimeOffset now)
    {
        EnsureStarted(now);

        if (_reaction is not null && now < _reactionUntil)
        {
            return _reaction.Value;
        }

        if (_reaction is not null)
        {
            _reaction = null;
            _reactionPriority = 0;
        }

        if (_pointerOver)
        {
            return PetState.Expecting;
        }

        if (now < _directUntil)
        {
            return PetState.Direct;
        }

        var inactiveFor = now - _lastInteractionAt;
        if (inactiveFor >= _options.BoredAfter)
        {
            return PetState.Bored;
        }

        if (inactiveFor >= _options.IdleAfter)
        {
            return SelectIdleVariant(inactiveFor - _options.IdleAfter);
        }

        return PetState.Direct;
    }

    private void TriggerReaction(
        PetState state,
        int priority,
        DateTimeOffset now,
        TimeSpan duration)
    {
        if (_reaction is not null && now < _reactionUntil && _reactionPriority > priority)
        {
            return;
        }

        _reaction = state;
        _reactionPriority = priority;
        _reactionUntil = now + duration;
        _lastInteractionAt = now;
        _directUntil = _reactionUntil + _options.RecentInteractionDirect;
    }

    private PetState SelectIdleVariant(TimeSpan elapsed)
    {
        var durationTicks = Math.Max(_options.IdleVariantDuration.Ticks, 1);
        var index = (int)((elapsed.Ticks / durationTicks) % 3);
        return index switch
        {
            0 => PetState.IdleCalm,
            1 => PetState.IdleCurious,
            _ => PetState.IdleSleepy,
        };
    }

    private void EnsureStarted(DateTimeOffset now)
    {
        if (!_started)
        {
            Start(now);
        }
    }
}

