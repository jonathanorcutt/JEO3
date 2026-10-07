namespace JEO3.Generation.Models
{
    public sealed class QueryGenerationMetricScore
    {
        public double ConfidenceScore { get; private init; } = 0.0;
        public IReadOnlyList<QueryGenerationMetricScoreDetailEntry> Details { get; private init; }

        public QueryGenerationMetricScore(double topConfidencePercentage, IReadOnlyList<QueryGenerationMetricScoreDetailEntry> scoreDetails)
        {
            ConfidenceScore = topConfidencePercentage;
            Details = scoreDetails;
        }
    }
}
