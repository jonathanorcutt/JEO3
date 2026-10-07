namespace JEO3.Generation.Models.Options
{
    public static class QueryGenerationOptionsPresets
    {
        public static QueryGenerationOptions JEO3 =>
            new QueryGenerationOptions(GenerationMethod.PlanDrivenExecution, true, false, ExecutionRoutingMode.OptimizeAllPermutations,
                new ProjectionSettings(TableSelectPolicy.AllColumns, TableSelectPolicy.None, true, 200),
                new FormattingSettings(AnnotationVerbosity.ExecutionSummary | AnnotationVerbosity.TraversalSummary | AnnotationVerbosity.DoNotRemoveAscii | AnnotationVerbosity.PathBreadcrumbs | AnnotationVerbosity.TableFooter, true, 30),
                new TraversalSettings(10, 10, true, false, false, true, false, false, new(), new()));
        public static QueryGenerationOptions FullProjection =>
            new QueryGenerationOptions(GenerationMethod.PlanDrivenExecution, true, false, ExecutionRoutingMode.OptimizeAllPermutations,
                new ProjectionSettings(TableSelectPolicy.AllColumns, TableSelectPolicy.None, true, 200),
                new FormattingSettings(AnnotationVerbosity.ExecutionSummary | AnnotationVerbosity.TraversalSummary | AnnotationVerbosity.DoNotRemoveAscii | AnnotationVerbosity.PathBreadcrumbs | AnnotationVerbosity.TableFooter, true, 30),
                new TraversalSettings(10, 10, true, false, false, true, false, false, new(), new()));
    }
}
