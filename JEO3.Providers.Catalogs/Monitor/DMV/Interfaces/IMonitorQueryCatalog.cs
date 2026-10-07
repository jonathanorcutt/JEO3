namespace JEO3.Providers.Catalogs
{
    public interface IMonitorQueryCatalog
    {
        string ActiveRequestsQuery { get; }
        string WaitStatsSnapshotQuery { get; }
        string BlockingQuery { get; }
        string TopExpensiveQueryQuery { get; }
        string PerformanceMetricQuery { get; }
        string GetDeadlockQuery(DateTime sinceUtc);
        string GetQuery(MonitorQueryType type, string sortColumn = "");
    }
}
