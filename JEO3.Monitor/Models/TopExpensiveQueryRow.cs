using System.Text.Json.Serialization;

namespace JEO3.Monitor.Models
{
    public sealed class TopExpensiveQueryRow
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("S")]
        public string SqlHandleHex { get; set; } = string.Empty;
        [JsonPropertyName("P")]
        public string PlanHandleHex { get; set; } = string.Empty;
        [JsonPropertyName("D")]
        public string DatabaseName { get; set; } = string.Empty;
        [JsonPropertyName("Q")]
        public string QueryText { get; set; } = string.Empty;
        [JsonPropertyName("E")]
        public long ExecutionCount { get; set; }

        // Duration / Elapsed Time (Microseconds to Milliseconds)
        [JsonPropertyName("TE")]
        public double TotalElapsedMs { get; set; }
        [JsonPropertyName("A")]
        public double AvgElapsedMs { get; set; }
        [JsonPropertyName("M")]
        public double MaxElapsedMs { get; set; }

        // CPU / Worker Time (Microseconds to Milliseconds)
        [JsonPropertyName("TC")]
        public double TotalCpuMs { get; set; }
        [JsonPropertyName("AC")]
        public double AvgCpuMs { get; set; }
        [JsonPropertyName("MC")]
        public double MaxCpuMs { get; set; }

        // I/O Metrics (Logical Reads & Writes)
        [JsonPropertyName("TL")]
        public long TotalLogicalReads { get; set; }
        [JsonPropertyName("AL")]
        public double AvgLogicalReads { get; set; }
        [JsonPropertyName("ML")]
        public long MaxLogicalReads { get; set; }

        [JsonPropertyName("LW")]
        public long TotalLogicalWrites { get; set; }

        [JsonPropertyName("AW")]
        public double AvgLogicalWrites { get; set; }
        [JsonPropertyName("MW")]
        public long MaxLogicalWrites { get; set; }

        // Combined I/O
        [JsonPropertyName("TI")]
        public long TotalIo { get; set; }
        [JsonPropertyName("AI")]
        public double AvgIo { get; set; }
        [JsonPropertyName("MI")]
        public long MaxIo { get; set; }

        // Redgate Query Impact score
        [JsonPropertyName("QI")]
        public double QueryImpact { get; set; }
        [JsonPropertyName("LE")]
        public DateTime LastExecutionTime { get; set; }
    }
}