namespace JEO3.Diagrams
{
    public sealed class DiagramEdgeViewModel
    {
        public Guid SourceNodeId { get; init; }
        public Guid TargetNodeId { get; init; }
        public string RelationshipName { get; init; }
        public string JoinType { get; init; }
    }
}
