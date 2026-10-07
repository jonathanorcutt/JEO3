using JEO3.Monitor.Models;

namespace JEO3.Monitor
{
    public sealed class MonitorHistorySample
    {
        public DateTime Timestamp { get; set; }
        public PerformanceMetricRow? Performance { get; set; }
        public List<TopExpensiveQueryRow> TopExpensiveQueries { get; set; } = [];
    }
}
