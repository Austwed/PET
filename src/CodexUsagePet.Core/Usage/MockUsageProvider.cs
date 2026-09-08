using CodexUsagePet.Core.Models;

namespace CodexUsagePet.Core.Usage;

public sealed class MockUsageProvider : IUsageProvider
{
    private readonly object _gate = new();
    private int _fiveHourRemaining = 86;
    private int _weeklyRemaining = 96;
    private readonly DateTimeOffset _fiveHourReset = DateTimeOffset.Now.AddHours(3);
    private readonly DateTimeOffset _weeklyReset = DateTimeOffset.Now.AddDays(5);

    public void SimulateConsumption(int fiveHourPoints = 7, int weeklyPoints = 2)
    {
        lock (_gate)
        {
            _fiveHourRemaining = Math.Clamp(_fiveHourRemaining - fiveHourPoints, 0, 100);
            _weeklyRemaining = Math.Clamp(_weeklyRemaining - weeklyPoints, 0, 100);
        }
    }

    public ValueTask<UsageSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var snapshot = new UsageSnapshot(
                SchemaVersion: 1,
                Source: "mock",
                SourceVersion: "phase-1",
                ObservedAt: DateTimeOffset.Now,
                Status: UsageAvailability.Available,
                FiveHour: UsageWindowSnapshot.FromUsedPercent(
                    300,
                    100 - _fiveHourRemaining,
                    _fiveHourReset),
                Weekly: UsageWindowSnapshot.FromUsedPercent(
                    10080,
                    100 - _weeklyRemaining,
                    _weeklyReset));

            return ValueTask.FromResult(snapshot);
        }
    }
}

