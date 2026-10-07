using System.Text.Json.Serialization;

namespace JEO3.Monitor
{
    public sealed class ActiveRequestRow
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("S")]
        public int SessionId { get; set; }
        [JsonPropertyName("BS")]
        public int BlockingSessionId { get; set; }
        [JsonPropertyName("WT")]
        public string WaitType { get; set; } = string.Empty;
        [JsonPropertyName("WTS")]
        public double WaitTimeSec { get; set; }
        [JsonPropertyName("ST")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("C")]
        public string Command { get; set; } = string.Empty;
        [JsonPropertyName("D")]
        public string DatabaseName { get; set; } = string.Empty;
        [JsonPropertyName("L")]
        public string LoginName { get; set; } = string.Empty;
        [JsonPropertyName("H")]
        public string HostName { get; set; } = string.Empty;
        [JsonPropertyName("P")]
        public string ProgramName { get; set; } = string.Empty;
        [JsonPropertyName("CP")]
        public long CpuTimeMs { get; set; }
        [JsonPropertyName("LR")]
        public long LogicalReads { get; set; }
        [JsonPropertyName("PR")]
        public long PhysicalReads { get; set; }
        [JsonPropertyName("W")]
        public long Writes { get; set; }
        [JsonPropertyName("O")]
        public int OpenTxnCount { get; set; }
        [JsonPropertyName("PCT")]
        public double PercentComplete { get; set; }
        [JsonPropertyName("STT")]
        public string StatementText { get; set; } = string.Empty;
        [JsonPropertyName("IB")]
        public bool IsBlocked => BlockingSessionId > 0;
    }
}
