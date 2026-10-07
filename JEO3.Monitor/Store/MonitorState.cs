using JEO3.Monitor.Models;

namespace JEO3.Monitor
{
    /// <summary>
    /// Live monitoring state for the SQL Server performance monitor panel.
    /// Added to DashboardWorkspace so components subscribe via OnMonitorStateChanged.
    /// </summary>
    public sealed class MonitorState
    {
        public MonitorPollingStatus Status { get; set; } = MonitorPollingStatus.Stopped;

        /// <summary>
        /// Non-null when a transient query failure occurred.
        /// The service keeps polling; this is cleared on next successful sample.
        /// </summary>
        public string? LastError { get; set; }
        public WaitRateSample? Latest => History.Count > 0 ? History[^1] : null;
        /// <summary>
        /// Rolling ring buffer of the last N wait-rate samples, oldest first.
        /// Capacity controlled by SqlServerMonitorService.HistoryCapacity.
        /// </summary>
        public IReadOnlyList<WaitRateSample> History { get; set; } = [];
        public IReadOnlyList<ActiveRequestRow> ActiveRequests { get; set; } = [];
        public IReadOnlyList<BlockingRow> BlockingChain { get; set; } = [];
        public IReadOnlyList<DeadlockEvent> Deadlocks { get; set; } = [];
        public IReadOnlyList<PerformanceMetricRow> PerformanceMetrics { get; set; } = [];
        public IReadOnlyList<TopExpensiveQueryRow> TopExpensiveQueries { get; set; } = [];
        public List<double> SparklinePoints { get; set; } = new();
    }
}
