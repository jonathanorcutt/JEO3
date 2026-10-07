namespace JEO3.Monitor
{
    public interface IMonitorApiClient
    {
        Task<MonitorSnapshot?> GetInitialSnapshotAsync(CancellationToken ct = default);
        Task<MonitorDeltaResponse?> GetDeltaAsync(DateTime sinceUtc, CancellationToken ct = default);
    }
}
