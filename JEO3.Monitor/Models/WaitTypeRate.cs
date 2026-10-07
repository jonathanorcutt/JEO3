using System.Text.Json.Serialization;

namespace JEO3.Monitor
{
    /// <summary>Per-wait-type rate contribution within a WaitRateSample.</summary>
    public sealed class WaitTypeRate
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("W")]
        public string WaitType { get; init; } = string.Empty;
        [JsonPropertyName("WM")]
        public double WaitMsPerSec { get; init; }
        [JsonPropertyName("WT")]
        public double WaitingTasksPerSec { get; init; }
        [JsonPropertyName("S")]
        public double SignalWaitMsPerSec { get; init; }

        /// <summary>Resource wait ms/s = WaitMsPerSec - SignalWaitMsPerSec.</summary>
        [JsonPropertyName("R")]
        public double ResourceWaitMsPerSec => WaitMsPerSec - SignalWaitMsPerSec;
    }
}
