namespace JEO3.Generation.Models
{
    public sealed class TraversalEvent
    {
        public int FromTableId { get; private init; }
        public int ToTableId { get; private init; }
        public int RelationId { get; private init; }
        public TraversalStopReason Reason { get; private init; }
        public TraversalDirection Direction { get; private init; }
        public int Depth { get; private init; }
        public DateTime Timestamp { get; private init; }

        public TraversalEvent(int fromTableId, int toTableId, int relationId, TraversalStopReason reason, TraversalDirection direction, int depth)
        {
            FromTableId = fromTableId;
            ToTableId = toTableId;
            RelationId = relationId;
            Reason = reason;
            Direction = direction;
            Depth = depth;
            Timestamp = DateTime.UtcNow;
        }

        public string LogMessage()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] Depth {Depth} | {Direction} | {FromTableId} -> {ToTableId} (Relation {RelationId}) | Stop: {Reason}";
        }
    }
}
