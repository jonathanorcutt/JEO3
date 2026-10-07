namespace JEO3.Generation.Models
{
    public sealed class InferredCandidate
    {
        public required GraphRelationConstraint Constraint { get; init; }
        public required double ConfidenceScore { get; init; }
        public required string Explanation { get; init; }
        public required string MatchingRule { get; init; }
    }
}
