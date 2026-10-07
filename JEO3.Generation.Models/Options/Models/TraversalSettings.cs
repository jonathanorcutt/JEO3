namespace JEO3.Generation.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TraversalSettings
    {
        #region Properties

        /// <summary>
        /// The number of levels up the schema graph to traverse when generating queries. This setting is relevant when the Strategy is set to FullGraph, as it determines how far up the traversal should go 
        /// into related tables and their relationships. A value of 1 means that only direct relationships will be considered, while higher values allow for deeper exploration of the schema graph. 
        /// Setting this to a higher number can lead to more complex queries and potentially longer generation times, especially in schemas with many relationships.
        /// </summary>
        public int LevelsUp { get; set; } = 1;

        /// <summary>
        ///  The number of levels down the schema graph to traverse when generating queries. This setting is relevant when the Strategy is set to FullGraph, as it determines how deep the traversal should go 
        ///  into related tables and their relationships. A value of 1 means that only direct relationships will be considered, while higher values allow for deeper exploration of the schema graph. 
        ///  Setting this to a higher number can lead to more complex queries and potentially longer generation times, especially in schemas with many relationships.
        /// </summary>
        public int LevelsDown { get; set; } = 1;

        /// <summary>
        /// Halts compilation loops if a cyclic reference pattern is identified across schema trees.
        /// Not Yet Implemented - Will Be Used To Prevent Infinite Loops In Cases Where Cycles Are Present In The Schema Graph And The Strategy Is Set To FullGraph
        /// </summary>
        public bool StopOnCycles { get; set; } = true;
        public bool IgnoreNullableForeignKeys { get; set; } = true;
        public bool IgnoreTablesWithZeroRows { get; set; } = false;
        /// <summary>
        /// Prevents the engine from traversing back down into any table that shares 
        /// the same TableName/ObjectId as the Root table during down-steps.
        /// </summary>
        public bool PreventRootTypeRecursion { get; set; } = true;

        public bool StrictIndexMatchingOnly { get; set; } = true;

        // If Strict Mode is on, Bias MUST be false
        public bool BiasSmallerTableScans
        {
            get => StrictIndexMatchingOnly ? false : field;
            set;
        } = true;

        // If Strict Mode is on, Strict Cycle Path MUST be true
        public bool EnforceStrictCyclePath
        {
            get => StrictIndexMatchingOnly ? true : field;
            set;
        } = true;

        // If Strict Mode is on, Strict Leaf Collapse MUST be true
        public bool EnforceStrictLeafCollapse
        {
            get => StrictIndexMatchingOnly ? true : field;
            set;
        } = true;

        /// <summary>
        /// Validates and synchronizes the internal state before the compiler runs.
        /// </summary>
        public void EnforceStrictness()
        {
            if (StrictIndexMatchingOnly)
            {
                BiasSmallerTableScans = false;
                EnforceStrictCyclePath = true;
                EnforceStrictLeafCollapse = true;
            }
        }

        /// <summary>
        /// Explicit column targets to filter out from traversal paths (e.g. "ModifiedDate, ModifiedBy").
        /// </summary>
        public List<string> OmitFromSelectsList { get; init; } = [];

        /// <summary>
        /// 
        /// </summary>
        public List<GraphRelationConstraint> RelationConstraints { get; init; }  // JEO3 Note - Finish Omission Logic

        #endregion

        #region Initialization

        // Private constructor for handling list allocations cleanly
        public TraversalSettings(
            int levelsUp,
            int levelsDown,
            bool stopOnCycles,
            bool ignoreNullableForeignKeys,
            bool ignoreTablesWithZeroRows,
            bool preventRootTypeRecursion,
            bool strictIndexMatchingOnly,
            bool biasSmallerTableScans,
            List<string> omitFromSelectsList,
            List<GraphRelationConstraint> relationConstraints,
            // ADD THESE:
            bool enforceStrictCyclePath = true,
            bool enforceStrictLeafCollapse = true,
            bool enforceStrictDepthLimit = false
        )
        {
            LevelsUp = levelsUp;
            LevelsDown = levelsDown;
            StopOnCycles = stopOnCycles;
            IgnoreNullableForeignKeys = ignoreNullableForeignKeys;
            IgnoreTablesWithZeroRows = ignoreTablesWithZeroRows;
            StrictIndexMatchingOnly = strictIndexMatchingOnly;
            PreventRootTypeRecursion = preventRootTypeRecursion;
            BiasSmallerTableScans = biasSmallerTableScans;
            OmitFromSelectsList = omitFromSelectsList?.ToList() ?? [];
            RelationConstraints = relationConstraints?.ToList() ?? [];
        }

        #endregion
    }
}