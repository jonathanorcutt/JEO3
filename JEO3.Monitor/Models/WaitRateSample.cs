using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JEO3.Core.ORM;
using JEO3.Providers.Extensions;

namespace JEO3.Monitor
{
    /// <summary>
    /// A computed wait-rate observation derived from two successive WaitStatRow snapshots.
    /// 
    /// Measurement basis:
    ///   WaitTimeMsPerSec  = Δwait_time_ms   / Δelapsed_seconds
    ///   WaitingTasksPerSec = Δwaiting_tasks_count / Δelapsed_seconds
    ///
    /// where Δ = (sampleB value) − (sampleA value) for each wait type,
    /// and Δelapsed_seconds = (sampleB.Timestamp − sampleA.Timestamp).TotalSeconds.
    ///
    /// A negative delta (server restart reset counters) is treated as zero.
    /// </summary>
    [JeoTable(nameof(WaitRateSample), Schema = "Prod")]
    public class WaitRateSample
    {
        /// <summary>UTC timestamp of the second (more recent) snapshot.</summary>
        [JeoKey(IsDbGenerated = false, IsPrimaryKey = true, Name = nameof(Timestamp))]
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Total wait-time rate across all non-benign wait types, in ms/s.
        /// This is the primary headline metric shown in the UI.
        /// </summary>
        [JsonPropertyName("W")]
        public double TotalWaitMsPerSec { get; init; }

        /// <summary>Total waiting task events per second across all non-benign waits.</summary>
        [JsonPropertyName("WT")]
        public double TotalWaitingTasksPerSec { get; init; }

        /// <summary>Elapsed seconds between the two snapshots used to compute this rate.</summary>
        [JsonPropertyName("E")]
        public double ElapsedSeconds { get; init; }

        /// <summary>Per-wait-type breakdown, sorted descending by WaitMsPerSec.</summary>
        [JsonPropertyName("TW")]
        public IReadOnlyList<WaitTypeRate> TopWaits { get; init; } = [];
    }
}
