namespace JEO3.Providers.Catalogs
{
    public abstract class BaseMonitorQueryCatalog : IMonitorQueryCatalog
    {
        public abstract string ActiveRequestsQuery { get; }
        public abstract string WaitStatsSnapshotQuery { get; }
        public abstract string BlockingQuery { get; }
        public abstract string TopExpensiveQueryQuery { get; }
        public abstract string PerformanceMetricQuery { get; }
        public virtual string GetDeadlockQuery(DateTime sinceUtc) => MSMonitorQueries.GetDeadlockQuery(sinceUtc);

        public string GetQuery(MonitorQueryType queryType, string sortColumn = "")
        {
            switch (queryType)
            {
                case MonitorQueryType.ActiveRequestsQuery:
                    return ActiveRequestsQuery;
                case MonitorQueryType.WaitStatsSnapshotQuery:
                    return WaitStatsSnapshotQuery;
                case MonitorQueryType.BlockingQuery:
                    return BlockingQuery;
                case MonitorQueryType.TopExpensiveQueryQuery:
                    return TopExpensiveQueryQuery.Replace("{0}", "AvgElapsed").Replace("@TopCount", "50");
                case MonitorQueryType.PerformanceMetricQuery:
                    return PerformanceMetricQuery;

            }
            return string.Empty;
        }
    }
}
