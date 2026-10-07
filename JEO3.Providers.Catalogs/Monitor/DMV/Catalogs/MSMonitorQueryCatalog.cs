namespace JEO3.Providers.Catalogs
{
    public sealed class MSMonitorQueryCatalog : BaseMonitorQueryCatalog
    {
        public override string ActiveRequestsQuery => MSMonitorQueries.ActiveRequestsQuery;
        public override string WaitStatsSnapshotQuery => MSMonitorQueries.WaitStatsSnapshotQuery;
        public override string BlockingQuery => MSMonitorQueries.BlockingQuery;
        public override string TopExpensiveQueryQuery => MSMonitorQueries.TopExpensiveQueryQuery;
        public override string PerformanceMetricQuery => MSMonitorQueries.PerformanceMetricQuery;
        public override string GetDeadlockQuery(DateTime sinceUtc) => MSMonitorQueries.GetDeadlockQuery(sinceUtc);
    }
}
