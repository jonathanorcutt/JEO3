using System.Text.Json.Serialization;

namespace JEO3.Monitor.Models;

public sealed class PerformanceMetricRow
{
    [JsonPropertyName("T")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("C")]
    public double CpuPercent { get; set; }
    [JsonPropertyName("M")]
    public double MemoryUsagePercent { get; set; }
    [JsonPropertyName("S")]
    public double TotalServerMemoryMb { get; set; }
    [JsonPropertyName("TS")]
    public double TargetServerMemoryMb { get; set; }
    [JsonPropertyName("MU")]
    public double SqlMemoryUsageMb { get; set; }
    [JsonPropertyName("P")]
    public double PageLifeExpectancy { get; set; }
    [JsonPropertyName("IR")]
    public double IoStallReadMs { get; set; }
    [JsonPropertyName("IW")]
    public double IoStallWriteMs { get; set; }
    [JsonPropertyName("B")]
    public double BatchRequestsPerSec { get; set; }
    [JsonPropertyName("U")]
    public double UserConnections { get; set; }
}
