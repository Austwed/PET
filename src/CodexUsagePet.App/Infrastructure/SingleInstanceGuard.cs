namespace CodexUsagePet.App.Infrastructure;

public sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex? _mutex;

    public SingleInstanceGuard(string name)
    {
        _mutex = new Mutex(initiallyOwned: true, name, out var createdNew);
        IsPrimaryInstance = createdNew;
    }

    public bool IsPrimaryInstance { get; }

    public void Dispose()
    {
        if (IsPrimaryInstance)
        {
            _mutex?.ReleaseMutex();
        }

        _mutex?.Dispose();
    }
}

