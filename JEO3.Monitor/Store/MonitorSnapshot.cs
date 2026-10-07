using System.Text.Json.Serialization;
using JEO3.Monitor.Models;

namespace JEO3.Monitor
{
    public sealed class MonitorSnapshot
    {
        public DateTime ServerTimeUtc { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MonitorPollingStatus Status { get; init; }
        public string? LastError { get; init; }
        public IReadOnlyList<WaitRateSample> History { get; init; } = [];
        public IReadOnlyList<ActiveRequestRow> ActiveRequests { get; init; } = [];
        public IReadOnlyList<BlockingRow> BlockingChain { get; init; } = [];
        public IReadOnlyList<DeadlockEvent> RecentDeadlocks { get; init; } = [];
        public IReadOnlyList<TopExpensiveQueryRow> TopExpensiveQueries { get; init; } = [];
        public IReadOnlyList<PerformanceMetricRow> PerformanceMetrics { get; init; } = [];
        public IReadOnlyList<MonitorHistorySample> PerformanceHistory { get; init; } = [];
        public WaitRateSample? LatestWaitRateSample => History.Count > 0 ? History[^1] : null;
        public PerformanceMetricRow? LatestPerformanceHistory => PerformanceMetrics.Count > 0 ? PerformanceMetrics?.FirstOrDefault() : null;
        public DeadlockEvent? LatestRecentDeadlock => RecentDeadlocks.Count > 0 ? RecentDeadlocks?.FirstOrDefault() : null;
        public BlockingRow? LatestBlockingChain => BlockingChain?.Count > 0 ? BlockingChain?.FirstOrDefault() : null;
        public ActiveRequestRow? LatestActiveRequest => ActiveRequests?.Count > 0 ? ActiveRequests?.FirstOrDefault() : null;
        public TopExpensiveQueryRow? LatestTopExpensiveQuery => TopExpensiveQueries?.Count > 0 ? TopExpensiveQueries?.FirstOrDefault() : null;
    }
}
