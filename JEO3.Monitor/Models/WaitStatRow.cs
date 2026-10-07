using System.Text.Json.Serialization;

namespace JEO3.Monitor
{
    // ── Data source contract — YOU implement this, service never touches SQL ──
    /// <summary>
    /// One row from sys.dm_os_wait_stats as captured by WaitStatsSnapshotQuery.
    /// Two successive snapshots are diffed by SqlServerMonitorService to compute rates.
    /// </summary>
    public sealed class WaitStatRow
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("W")]
        public string WaitType { get; set; } = string.Empty;
        [JsonPropertyName("WC")]
        public long WaitingTasksCount { get; set; }
        [JsonPropertyName("WT")]
        public long WaitTimeMs { get; set; }
        [JsonPropertyName("M")]
        public long MaxWaitTimeMs { get; set; }
        [JsonPropertyName("S")]
        public long SignalWaitTimeMs { get; set; }
    }
}
