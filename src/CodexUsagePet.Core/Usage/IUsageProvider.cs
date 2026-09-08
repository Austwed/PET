using CodexUsagePet.Core.Models;

namespace CodexUsagePet.Core.Usage;

public interface IUsageProvider
{
    ValueTask<UsageSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
}

