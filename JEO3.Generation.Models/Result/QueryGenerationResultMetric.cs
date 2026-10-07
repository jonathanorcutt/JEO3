namespace JEO3.Generation.Models
{
    public sealed class QueryGenerationResultMetric
    {
        #region Properties

        public QueryGenerationMetricScore Score { get; set; }

        public int DuplicateCount => DuplicateDetails == null ? 0 : DuplicateDetails.Count;
        public List<string> DuplicateDetails { get; init; } = [];
        public int JoinCount { get; init; }

        public double AverageTraversalDepth { get; init; }
        public int MaxTraversalDepth { get; init; }
        public bool HitDepthLimit { get; init; }
        public int NodesVisited { get; init; }

        #endregion

        #region Initialization

        public QueryGenerationResultMetric(int nodesVisited, int maxTraversalDepth, double avgTraversalDepth, bool hitDepthLimit, int joinCount, List<string> duplicateDetails)
        {
            NodesVisited = nodesVisited;
            MaxTraversalDepth = maxTraversalDepth;
            AverageTraversalDepth = avgTraversalDepth;
            HitDepthLimit = hitDepthLimit;
            JoinCount = joinCount;
            DuplicateDetails = duplicateDetails;

            Score = this.CalculateConfidence();
        }

        #endregion

        #region Confidence

        // Calculation Matrix (0 - 100%)
        private QueryGenerationMetricScore CalculateConfidence()
        {
            var list = new List<QueryGenerationMetricScoreDetailEntry>();

            // Establish a base maximum confidence budget
            double score = 100.0;

            // New logic: If we hit a depth limit, we can penalize or flag the result
            if (HitDepthLimit)
            {
                score -= 10.0; // "Incomplete traversal" penalty
                list.Add(new QueryGenerationMetricScoreDetailEntry(-10.0, $"We hit a depth limit ({this.MaxTraversalDepth}). Incomplete traversal penalty: -10.0"));
            }

            // Penalty 1: Traversal explosion boundaries
            if (JoinCount == 0)
            {
                score -= 15; // Lacks relationship context entirely
                list.Add(new QueryGenerationMetricScoreDetailEntry(-10.0, $"Generated query has 0 joins. Lacks relationship context entirely: -15.0"));
            }
            else if (JoinCount > 25)
            {

                //score -= (JoinCount - 25) * 2.5; // Severe penalty for join bloat
                //list.Add(new QueryGenerationMetricScoreDetailEntry(-((JoinCount - 25) * 2.5), $"We exceeded the hard join count penalty default threshold of 25. Excessive joins: ({JoinCount}). Join bloat penalty: ({JoinCount} - 25) * 2.5 = {-((JoinCount - 25) * 2.5)}"));
            }
            else if (JoinCount > 12)
            {
                //score -= (JoinCount - 12) * 1.5; // Soft penalty for complex execution plans
                //list.Add(new QueryGenerationMetricScoreDetailEntry(-((JoinCount - 12) * 1.5), $"We exceeded the soft join count penalty default threshold of 12. Actual joins: {JoinCount}. Penalty: ({JoinCount} - 12) * 1.5: {-((JoinCount - 12) * 1.5)}"));
            }

            // Penalty 2: Excessive loop paths (e.g., BillTo/ShipTo explosions)
            if (DuplicateCount > 0)
            {
                score -= DuplicateCount * 8.0; // Strict structural reduction per loop intersection
                list.Add(new QueryGenerationMetricScoreDetailEntry(-(DuplicateCount * 8.0), $"Strict structural reduction per loop intersection - We found excessive loop paths. Duplicate joins found: {DuplicateCount}. Penalty: {DuplicateCount} * 8.0 = {-(DuplicateCount * 8.0)}"));
            }

            // New logic: Use Traversal Depth as a "Complexity Multiplier"
            if (MaxTraversalDepth > 5)
            {
                score -= (MaxTraversalDepth - 5) * 5.0; // Deeper trees = higher risk of cartesian products
                list.Add(new QueryGenerationMetricScoreDetailEntry(-((MaxTraversalDepth - 5) * 5.0), $"Traversal Depth as a 'Complexity Multiplier' (Deeper trees = higher risk of cartesian products). Our maximum depth of {MaxTraversalDepth} is greater than traversal depth penalty default threshold of 5. Penalty: ({MaxTraversalDepth} - 5) * 5.0 = {-((MaxTraversalDepth - 5) * 5.0)}"));
            }

            // Guard rails
            return new QueryGenerationMetricScore(Math.Clamp(score, 0.0, 100.0), list);
        }

        #endregion
    }
}
