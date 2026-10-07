using System.Text.Json.Serialization;

namespace JEO3.Monitor
{
    public sealed class BlockingRow
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("S")]
        public int SessionId { get; set; }
        [JsonPropertyName("B")]
        public int BlockingSessionId { get; set; }
        [JsonPropertyName("IB")]
        public bool IsBlocked { get; set; }
        [JsonPropertyName("ST")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("L")]
        public string LoginName { get; set; } = string.Empty;
        [JsonPropertyName("H")]
        public string HostName { get; set; } = string.Empty;
        [JsonPropertyName("P")]
        public string ProgramName { get; set; } = string.Empty;
        [JsonPropertyName("C")]
        public string Command { get; set; } = string.Empty;
        [JsonPropertyName("W")]
        public string WaitType { get; set; } = string.Empty;
        [JsonPropertyName("WT")]
        public double WaitTimeSec { get; set; }
        [JsonPropertyName("R")]
        public DateTime? RequestStartTime { get; set; }
        [JsonPropertyName("TR")]
        public DateTime? TransactionBeginTime { get; set; }
        [JsonPropertyName("O")]
        public double? OpenTranDurationSec { get; set; }
        [JsonPropertyName("Q")]
        public string QueryText { get; set; } = string.Empty;
        [JsonPropertyName("I")]
        public bool IsVictim { get; set; }
    }
}
