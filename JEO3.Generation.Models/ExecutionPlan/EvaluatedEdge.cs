using JEO3.Schema;


namespace JEO3.Generation.Models
{
    public sealed class EvaluatedEdge
    {
        public double Score { get; internal set; }
        public int WeightPenalty { get; internal set; }
        public bool UsesIdentityKey { get; internal set; }
        public bool UsesComputedColumn { get; internal set; }
        public bool UsesCompositeKey { get; internal set; }
        public bool UsesNullableForeignKey { get; internal set; }
        public bool HasDatatypeMismatch { get; internal set; }
        public bool IsCoveringIndex { get; internal set; }
        public List<string> Reasons { get; } = [];
        public required IRelation Relationship { get; init; }
        public JoinEfficiency Efficiency { get; internal set; }
    }
}