using JEO3.Monitor;

namespace JEO3.Site.Components.ECharts
{
    public interface IMonitorDataService
    {
        MonitorSnapshot? CurrentSnapshot { get; }
        event Action<MonitorDeltaResponse>? OnDelta;
        Task EnsureStartedAsync();
    }
}
