using JEO3.Core;

namespace JEO3.Providers.Catalogs
{
    public abstract class BaseMonitorQueryStoreCatalog : IMonitorQueryStoreCatalog
    {
        public abstract string GetActiveRequestsQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetWaitSamplesQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetWaitTypesQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetBlockingQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetTopExpensiveQueryQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetPerformanceMetricQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetDeadlockEventQuery(EnvironmentType environment, DateTime utcStartTime);
        public abstract string GetDeadlockParticipantQuery(EnvironmentType environment, DateTime utcStartTime);
        public string GetQuery(MonitorStoreQueryType type, DateTime utcStartTime, EnvironmentType environment, string sortColumn = "")
        {
            {
                switch (type)
                {
                    case MonitorStoreQueryType.ActiveRequestsQuery:
                        return GetActiveRequestsQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.WaitSamplesQuery:
                        return GetWaitSamplesQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.WaitTypesQuery:
                        return GetWaitTypesQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.BlockingQuery:
                        return GetBlockingQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.TopExpensiveQueryQuery:
                        return GetTopExpensiveQueryQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.PerformanceMetricQuery:
                        return GetPerformanceMetricQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.DeadlockEventQuery:
                        return GetDeadlockEventQuery(environment, utcStartTime);
                    case MonitorStoreQueryType.DeadlockParticipantQuery:
                        return GetDeadlockParticipantQuery(environment, utcStartTime);
                }
                return string.Empty;
            }
        }
    }
}
