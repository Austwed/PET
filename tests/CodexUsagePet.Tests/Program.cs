using CodexUsagePet.Core.Behavior;
using CodexUsagePet.Core.Models;
using CodexUsagePet.Core.Usage;

namespace CodexUsagePet.Tests;

internal static class Program
{
    private static readonly DateTimeOffset Epoch = new(2026, 9, 9, 0, 0, 0, TimeSpan.Zero);

    public static int Main()
    {
        var tests = new (string Name, Action Run)[]
        {
            ("Initial snapshot establishes a baseline", InitialSnapshotDoesNotNotify),
            ("Crossing 80 triggers once", CrossingEightyTriggersOnce),
            ("One refresh can cross multiple thresholds", MultipleThresholdsCollapsePerWindow),
            ("Both windows can react together", BothWindowsReactTogether),
            ("A reset starts a clean period", ResetStartsCleanPeriod),
            ("Stale data cannot trigger or replace baseline", StaleDataIsIgnored),
            ("Pet advances through direct, idle variants, and bored", StateMachineTimeProgression),
            ("Hover and head pat follow priority", HoverAndHeadPatPriority),
            ("Usage crying overrides interaction", CryingOverridesInteraction),
        };

        var failures = 0;
        foreach (var test in tests)
        {
            try
            {
                test.Run();
                Console.WriteLine($"PASS  {test.Name}");
            }
            catch (Exception exception)
            {
                failures++;
                Console.Error.WriteLine($"FAIL  {test.Name}: {exception.Message}");
            }
        }

        Console.WriteLine($"{tests.Length - failures}/{tests.Length} tests passed.");
        return failures == 0 ? 0 : 1;
    }

    private static void InitialSnapshotDoesNotNotify()
    {
        var engine = new ThresholdEngine();
        Assert(!engine.Evaluate(Snapshot(76, 94)).ShouldReact, "Initial data must be silent.");
    }

    private static void CrossingEightyTriggersOnce()
    {
        var engine = new ThresholdEngine();
        engine.Evaluate(Snapshot(81, 94));
        var result = engine.Evaluate(Snapshot(80, 94));
        Assert(result.Events.Count == 1, "Expected one five-hour event.");
        Assert(result.Events[0].LowestCrossedThreshold == 80, "Expected the 80% level.");
        Assert(!engine.Evaluate(Snapshot(79, 94)).ShouldReact, "The same level must not repeat.");
    }

    private static void MultipleThresholdsCollapsePerWindow()
    {
        var engine = new ThresholdEngine();
        engine.Evaluate(Snapshot(61, 94));
        var result = engine.Evaluate(Snapshot(39, 94));
        Assert(result.Events.Count == 1, "Crossings in one window should collapse.");
        Assert(result.Events[0].LowestCrossedThreshold == 40, "Lowest crossed level should be 40%.");
        Assert(result.Events[0].CrossedThresholds.SequenceEqual([60, 40]), "Both levels must be recorded.");
    }

    private static void BothWindowsReactTogether()
    {
        var engine = new ThresholdEngine();
        engine.Evaluate(Snapshot(42, 82));
        var result = engine.Evaluate(Snapshot(19, 79));
        Assert(result.Events.Count == 2, "Expected one event per window.");
        Assert(result.Events.Any(item => item.Window == UsageWindowKind.FiveHour), "Missing five-hour event.");
        Assert(result.Events.Any(item => item.Window == UsageWindowKind.Weekly), "Missing weekly event.");
    }

    private static void ResetStartsCleanPeriod()
    {
        var engine = new ThresholdEngine();
        var firstReset = Epoch.AddHours(5);
        var secondReset = Epoch.AddHours(10);
        engine.Evaluate(Snapshot(2, 90, firstReset));
        var reset = engine.Evaluate(Snapshot(100, 90, secondReset));
        Assert(!reset.ShouldReact, "A reset must not cause crying.");
        var afterReset = engine.Evaluate(Snapshot(79, 90, secondReset));
        Assert(afterReset.Events.Single().LowestCrossedThreshold == 80, "New period should re-enable thresholds.");
    }

    private static void StaleDataIsIgnored()
    {
        var engine = new ThresholdEngine();
        engine.Evaluate(Snapshot(81, 90));
        Assert(!engine.Evaluate(Snapshot(0, 0, status: UsageAvailability.Stale)).ShouldReact, "Stale data must be silent.");
        Assert(engine.Evaluate(Snapshot(80, 90)).Events.Single().LowestCrossedThreshold == 80,
            "Stale data must not replace the valid baseline.");
    }

    private static void StateMachineTimeProgression()
    {
        var options = TestTimingOptions();
        var machine = new PetStateMachine(options);
        machine.Start(Epoch);
        Assert(machine.GetState(Epoch.AddSeconds(7)) == PetState.Direct, "Summon should remain direct.");
        Assert(machine.GetState(Epoch.AddSeconds(21)) == PetState.IdleCalm, "Expected calm idle first.");
        Assert(machine.GetState(Epoch.AddSeconds(29)) == PetState.IdleCurious, "Expected curious idle second.");
        Assert(machine.GetState(Epoch.AddSeconds(37)) == PetState.IdleSleepy, "Expected sleepy idle third.");
        Assert(machine.GetState(Epoch.AddSeconds(601)) == PetState.Bored, "Expected bored after ten minutes.");
    }

    private static void HoverAndHeadPatPriority()
    {
        var machine = new PetStateMachine(TestTimingOptions());
        machine.Start(Epoch);
        machine.PointerEntered(Epoch.AddMinutes(11));
        Assert(machine.GetState(Epoch.AddMinutes(11)) == PetState.Expecting, "Hover should show expecting.");
        machine.HeadPat(Epoch.AddMinutes(11));
        Assert(machine.GetState(Epoch.AddMinutes(11).AddSeconds(1)) == PetState.HeadPat, "Head pat should override hover.");
        Assert(machine.GetState(Epoch.AddMinutes(11).AddSeconds(3)) == PetState.Expecting, "Hover should resume after pat.");
    }

    private static void CryingOverridesInteraction()
    {
        var machine = new PetStateMachine(TestTimingOptions());
        machine.Start(Epoch);
        machine.PointerEntered(Epoch);
        machine.HeadPat(Epoch);
        machine.UsageThresholdCrossed(Epoch.AddMilliseconds(50));
        machine.HeadPat(Epoch.AddMilliseconds(100));
        Assert(machine.GetState(Epoch.AddSeconds(1)) == PetState.Crying, "Crying must have top priority.");
    }

    private static PetTimingOptions TestTimingOptions() => new(
        SummonDirect: TimeSpan.FromSeconds(8),
        RecentInteractionDirect: TimeSpan.FromSeconds(12),
        IdleAfter: TimeSpan.FromSeconds(20),
        BoredAfter: TimeSpan.FromMinutes(10),
        IdleVariantDuration: TimeSpan.FromSeconds(8),
        HeadPatDuration: TimeSpan.FromSeconds(2),
        CryingDuration: TimeSpan.FromSeconds(4));

    private static UsageSnapshot Snapshot(
        int fiveHourRemaining,
        int weeklyRemaining,
        DateTimeOffset? fiveHourReset = null,
        UsageAvailability status = UsageAvailability.Available)
    {
        fiveHourReset ??= Epoch.AddHours(5);
        return new UsageSnapshot(
            SchemaVersion: 1,
            Source: "mock",
            SourceVersion: "tests",
            ObservedAt: Epoch,
            Status: status,
            FiveHour: UsageWindowSnapshot.FromUsedPercent(300, 100 - fiveHourRemaining, fiveHourReset),
            Weekly: UsageWindowSnapshot.FromUsedPercent(10080, 100 - weeklyRemaining, Epoch.AddDays(7)));
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

