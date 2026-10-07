namespace JEO3.Generation.Models
{
    public sealed class QueryGenerationResult
    {
        #region Properties

        public Guid Id => Guid.NewGuid();
        public GenerationMethod Strategy { get; private init; }
        public string StrategyLabel => Strategy switch
        {
            GenerationMethod.FullGraph => "Full Graph",
            GenerationMethod.UpThenDown => "Up Then Down",
            GenerationMethod.RootSplit => "Root Split",
            GenerationMethod.Backtracking => "Backtracking",
            GenerationMethod.JEO3 => "JEO3",
            GenerationMethod.ExtendedDiscovery => "Extended Discovery",
            GenerationMethod.PlanDrivenExecution => "PlanDrivenExecution",
            _ => "Constrained Recursive Normalization"
        };
        public string MatrixEvaluationLabel
        {
            get
            {
                return $"{StrategyLabel} ({Metrics?.JoinCount ?? 0} Joins)";
            }
        }

        public string GeneratedQuery { get; init; } = string.Empty;

        public IReadOnlyList<QueryTable> ExecutionNodes { get; init; } = Array.Empty<QueryTable>();
        public QueryGenerationResultMetric Metrics { get; init; } = new(0, 0, 0, false, 0, new());
        public SchemaCoverageMetrics Coverage { get; set; } = new();
        public IQueryTracker Tracker { get; init; }

        #endregion

        #region Initialization
        public QueryGenerationResult() { }

        public QueryGenerationResult(
            string generatedQuery,
            QueryGenerationResultMetric metrics,
            IReadOnlyList<QueryTable> executionNodes,
            IQueryTracker tracker,
            GenerationMethod method)
        {
            GeneratedQuery = generatedQuery;
            Metrics = metrics;
            ExecutionNodes = executionNodes;
            Tracker = tracker;
            Strategy = method;
        }

        #endregion
    }
}