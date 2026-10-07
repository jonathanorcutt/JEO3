namespace JEO3.Monitor.ECharts
{
    public interface IMonitorChartDataService
    {
        MonitorSnapshot? CurrentSnapshot { get; }
        event Action<MonitorDeltaResponse>? OnDelta;
        Task EnsureStartedAsync();
    }
}
