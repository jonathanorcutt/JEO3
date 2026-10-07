using System.ComponentModel;

namespace JEO3.Generation
{
    // -------- Generation --------
    public enum ExecutionRoutingMode
    {
        OptimizeAllPermutations = 0,
        EnforceSelectedUiSettings = 1
    }
    public enum RootTraversalMethod
    {
        PrimaryKeyLookup,      // Target: 1 exact record
        SelectiveFilteredScan, // Target: Subset scan (e.g. Active status, date ranges)
        BroadIndexScan,        // Target: Wide index scan 
        FullTableScan          // Target: The entire table
    }
    public enum TraversalDirection
    {
        Root,
        Up,
        Down
    }
    public enum TableSelectPolicy
    {
        AllColumns,
        Star,
        PkOnly,
        None
    }
    public enum GenerationMethod
    {
        [Description("Plan Driven Execution")]
        PlanDrivenExecution = 0,
        [Description("Full Graph")]
        FullGraph = 1,
        [Description("Split Root")]
        RootSplit = 2,
        [Description("Up-Then-Down")]
        UpThenDown = 3,
        [Description("Backtracking")]
        Backtracking = 4,
        [Description("JEO3")]
        JEO3 = 5,
        [Description("Extended Discovery")]
        ExtendedDiscovery = 6
    }

    // -------- Formatting --------
    [Flags]
    public enum AnnotationVerbosity
    {
        None = 0,
        GenerationInfo = 1 << 0,
        DoNotRemoveAscii = 1 << 1,
        CardinalityEstimation = 1 << 2,
        TableFooter = 1 << 3,
        PathBreadcrumbs = 1 << 4,
        ExecutionSummary = 1 << 5,
        TraversalSummary = 1 << 6
    }

    // --------- Traversal --------
    public enum TraversalStopReason
    {
        None,
        MaxDepth,
        CycleDetected,
        RelationExcludedOmitted,
        RelationExcludedNullableKey
    }

    // ------ Execution Plan ------
    public enum JoinEfficiency
    {
        PerfectSeek = 0,     // Guarded by a unique or clustered index key
        CoveredScan = 1,     // Guarded by a non-unique index key / include columns
        UnindexedHazard = 2  // Complete blind table scan (No indexes match)
    }

    public enum DiscoveryMode
    {
        Jarvis,
        Strict,             // Global dedup active; paths are pruned immediately
        WideNetExtended     // Global dedup disabled during traversal; compression pass required
    }

    // -------- Diagnostics -------
    public enum SchemaFindingSeverity
    {
        Good,
        Info,
        Warning,
        High,
        Critical
    }

    public enum SchemaObjectType
    {
        Database,
        Schema,
        Table,
        View,
        Index,
        Column,
        ForeignKey,
        StoredProcedure,
        Constraint,
        Function,
        Trigger
    }

    // ---------- Drift -----------
    public enum DriftType
    {
        MissingInTarget,// E.g., Exists in Dev, missing in Prod (Needs CREATE)
        MissingInSource,// E.g., Exists in Prod, missing in Dev (Needs DROP)
        Modified,// E.g., Data type, length, or nullability changed (Needs ALTER)
        Informational,
        Warning,
        Critical
    }

    public enum IssueType
    {
        IdentityExhaustion,
        HeapTable,
        UnindexedForeignKey,
        RecentSchemaMutation,
        MemoryOptimized,
        DeprecatedDataType,
        OverIndexedTable
    }
}
