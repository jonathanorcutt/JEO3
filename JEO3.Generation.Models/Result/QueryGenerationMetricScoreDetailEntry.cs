namespace JEO3.Generation.Models
{
    public sealed class QueryGenerationMetricScoreDetailEntry
    {
        public double Delta { get; private init; }
        public string Message { get; private init; }

        public QueryGenerationMetricScoreDetailEntry(double delta, string message)
        {
            Delta = delta;
            Message = message;
        }
    }
}
