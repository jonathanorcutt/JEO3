using System.Text.Json.Serialization;
using JEO3.Core.ORM;
using JEO3.Providers.Extensions;

namespace JEO3.Monitor
{
    [JeoTable(nameof(DeadlockParticipant), Schema = "Prod")]
    public sealed class DeadlockParticipant
    {
        [JeoKey(IsDbGenerated = false, IsPrimaryKey = true, Name = nameof(Timestamp))]
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("S")]
        public int SessionId { get; init; }
        [JsonPropertyName("L")]
        public string LoginName { get; init; } = string.Empty;
        [JsonPropertyName("H")]
        public string HostName { get; init; } = string.Empty;
        [JsonPropertyName("P")]
        public string ProgramName { get; init; } = string.Empty;
        [JsonPropertyName("LS")]
        public string LastStatement { get; init; } = string.Empty;
        [JsonPropertyName("I")]
        public bool IsVictim { get; init; }
    }
}
