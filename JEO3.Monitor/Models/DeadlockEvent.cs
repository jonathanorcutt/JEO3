using System.Text.Json.Serialization;

namespace JEO3.Monitor
{
    public sealed class DeadlockEvent
    {
        [JsonPropertyName("T")]
        public DateTime Timestamp { get; set; }
        [JsonPropertyName("V")]
        public string VictimSessionId { get; init; }
        public IReadOnlyList<DeadlockParticipant> Participants { get; init; } = []; // Added explicit name 'Participants'
        [JsonPropertyName("R")]
        public string RawXml { get; init; } = "";
    }
}
