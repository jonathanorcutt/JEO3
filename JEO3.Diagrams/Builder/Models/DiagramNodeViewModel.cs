namespace JEO3.Diagrams
{
    public sealed class DiagramNodeViewModel
    {
        public Guid NodeId { get; init; }
        public string TableName { get; init; }
        public string Alias { get; init; }
        public int Depth { get; init; }
        public string SqlSafeName { get; init; }
    }
}
