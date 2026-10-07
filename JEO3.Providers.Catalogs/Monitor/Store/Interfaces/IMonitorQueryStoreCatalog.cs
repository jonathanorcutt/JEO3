using JEO3.Core;

namespace JEO3.Providers.Catalogs
{
    public interface IMonitorQueryStoreCatalog
    {
        string GetActiveRequestsQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetWaitSamplesQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetWaitTypesQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetBlockingQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetTopExpensiveQueryQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetPerformanceMetricQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetDeadlockEventQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetDeadlockParticipantQuery(EnvironmentType environment, DateTime utcStartTime);
        string GetQuery(MonitorStoreQueryType type, DateTime utcStartTime, EnvironmentType environment, string sortColumn = "");
    }
}
