namespace JEO3.Generation.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class QueryGenerationOptions
    {
        #region Properties

        public bool EnableGraphGeneration { get; set; } = true;
        public bool IncludeCTEs { get; set; } = false;


        /// <summary>
        ///  The strategy to use when generating queries based on the schema graph. The GenerationMethod enum likely defines different approaches for traversing the schema graph and generating queries, 
        ///  such as FullGraph, BreadthFirst, DepthFirst, etc. The default value is set to FullGraph, which suggests that the generator will attempt to create queries that encompass the entire schema graph, 
        ///  including all related tables and their relationships. Depending on the chosen strategy, the generator may produce different types of queries with varying levels of complexity and depth in terms 
        ///  of how they navigate the relationships between tables in the schema.
        /// </summary>
        public GenerationMethod Strategy { get; set; } = GenerationMethod.PlanDrivenExecution;
        public DiscoveryMode ExecutionPlan { get; set; } = DiscoveryMode.Jarvis;
        private ExecutionPlanStrategy _ExecutionPlanStrategy;
        public ExecutionPlanStrategy ExecutionPlanStrategy
        {
            get
            {
                return _ExecutionPlanStrategy;
            }
            set
            {
                _ExecutionPlanStrategy = value;
            }
        }

        public ProjectionSettings Retrieval { get; init; }
        public FormattingSettings Formatting { get; init; }
        public TraversalSettings Traversal { get; init; }
        public ExecutionRoutingMode RoutingMode { get; init; } = ExecutionRoutingMode.EnforceSelectedUiSettings;

        #endregion

        #region Initialization

        public QueryGenerationOptions(
            GenerationMethod strategy,
            bool enableGraphGeneration = true,
            bool includeCTEs = false,
            ExecutionRoutingMode routingMode = ExecutionRoutingMode.OptimizeAllPermutations,
            ProjectionSettings retrieval = null,
            FormattingSettings formatting = null,
            TraversalSettings traversal = null)
        {
            Strategy = strategy > 0 ? strategy : GenerationMethod.PlanDrivenExecution;
            EnableGraphGeneration = enableGraphGeneration;
            IncludeCTEs = includeCTEs;
            RoutingMode = routingMode;

            // Fallback to safe, parameterless-compatible baselines rather than null tokens
            Retrieval = retrieval ?? new ProjectionSettings(TableSelectPolicy.AllColumns,
                TableSelectPolicy.None,
                leftJoinsOnly: false,
                columnsPerTableLimit: 200);

            Formatting = formatting ?? new FormattingSettings(
                AnnotationVerbosity.GenerationInfo | AnnotationVerbosity.DoNotRemoveAscii,
                singleLinePerTable: true,
                breadcrumbPaddingRight: 50);

            Traversal = traversal ?? new TraversalSettings( // JEO3 Note - Finish Omission Logic
                levelsUp: 1,
                levelsDown: 1,
                stopOnCycles: true,
                ignoreNullableForeignKeys: false,
                ignoreTablesWithZeroRows: false,
                preventRootTypeRecursion: false,
                strictIndexMatchingOnly: false,
                biasSmallerTableScans: false,
                [],
                [],
                enforceStrictCyclePath: false,
                enforceStrictLeafCollapse: false,
                enforceStrictDepthLimit: false);
        }

        #endregion

        #region Default

        public static QueryGenerationOptions GetDefaultGenerationOptions(GenerationMethod method = GenerationMethod.PlanDrivenExecution)
        {
            var options = new QueryGenerationOptions(
             strategy: method,//options.QueryGenerationMethod,
             enableGraphGeneration: true,
             routingMode: ExecutionRoutingMode.EnforceSelectedUiSettings, // Every engine call is direct now
             retrieval: new ProjectionSettings(TableSelectPolicy.AllColumns, TableSelectPolicy.PkOnly, true, 1000),
             formatting: new FormattingSettings(AnnotationVerbosity.ExecutionSummary | AnnotationVerbosity.TraversalSummary | AnnotationVerbosity.PathBreadcrumbs | AnnotationVerbosity.TableFooter | AnnotationVerbosity.DoNotRemoveAscii, true, 30),
             traversal: new TraversalSettings(
                 levelsUp: 12,
                 levelsDown: 12,
                 stopOnCycles: true,
                 ignoreNullableForeignKeys: false,
                 ignoreTablesWithZeroRows: false,
                 preventRootTypeRecursion: false,
                 strictIndexMatchingOnly: false,
                 biasSmallerTableScans: false,
                 enforceStrictCyclePath: false,
                 enforceStrictLeafCollapse: false,
                 enforceStrictDepthLimit: false,
                 omitFromSelectsList: [],
                 relationConstraints: [] //.Concat(remaining) //.Concat(blindInferredConstraints) INNFERRED RELATIONS INJECTION EXISTS HERE!
                )
         );
            return options;
        }
        #endregion
    }
}