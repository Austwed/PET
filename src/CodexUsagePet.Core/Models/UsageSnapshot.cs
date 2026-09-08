namespace CodexUsagePet.Core.Models;

public enum UsageAvailability
{
    Available,
    Stale,
    Unavailable,
}

public enum UsageWindowKind
{
    FiveHour,
    Weekly,
}

public sealed record UsageWindowSnapshot(
    int WindowDurationMinutes,
    int UsedPercent,
    int RemainingPercent,
    DateTimeOffset? ResetsAt)
{
    public static UsageWindowSnapshot FromUsedPercent(
        int windowDurationMinutes,
        int usedPercent,
        DateTimeOffset? resetsAt)
    {
        var safeUsedPercent = Math.Clamp(usedPercent, 0, 100);
        return new UsageWindowSnapshot(
            windowDurationMinutes,
            safeUsedPercent,
            100 - safeUsedPercent,
            resetsAt);
    }
}

public sealed record UsageError(string Code, string Message);

public sealed record UsageSnapshot(
    int SchemaVersion,
    string Source,
    string? SourceVersion,
    DateTimeOffset ObservedAt,
    UsageAvailability Status,
    UsageWindowSnapshot? FiveHour,
    UsageWindowSnapshot? Weekly,
    UsageError? Error = null)
{
    public static UsageSnapshot Unavailable(
        string source,
        string code,
        string message,
        DateTimeOffset observedAt) =>
        new(
            SchemaVersion: 1,
            Source: source,
            SourceVersion: null,
            ObservedAt: observedAt,
            Status: UsageAvailability.Unavailable,
            FiveHour: null,
            Weekly: null,
            Error: new UsageError(code, message));
}

