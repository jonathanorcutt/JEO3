using JEO3.Monitor.Models;

namespace JEO3.Monitor
{
    public sealed class MonitorDeltaResponse
    {

        public DateTime ServerTimeUtc { get; set; }
        public MonitorPollingStatus Status { get; init; }
        public string? LastError { get; init; }

        // Time-series updates (only samples > sinceUtc)
        public IReadOnlyList<WaitRateSample> History { get; init; } = [];
        public IReadOnlyList<MonitorHistorySample> PerformanceHistory { get; init; } = [];
        public IReadOnlyList<DeadlockEvent> RecentDeadlocks { get; init; } = [];

        // Current transient state (always fresh)
        public IReadOnlyList<ActiveRequestRow> ActiveRequests { get; init; } = [];
        public IReadOnlyList<BlockingRow> BlockingChain { get; init; } = [];
        public IReadOnlyList<TopExpensiveQueryRow> TopExpensiveQueries { get; init; } = [];
    }
}
