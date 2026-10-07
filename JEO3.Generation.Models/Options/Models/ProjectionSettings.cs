namespace JEO3.Generation.Models
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProjectionSettings
    {
        #region Propeties

        /// <summary>
        /// The column selection policy applied specifically to the initial root driver table.
        /// </summary>
        public TableSelectPolicy RootSelectPolicy { get; set; } = TableSelectPolicy.AllColumns;

        /// <summary>
        /// The column selection policy applied to target tables joined through relations.
        /// </summary>
        public TableSelectPolicy JoinSelectPolicy { get; set; } = TableSelectPolicy.None;

        public bool LeftJoinsOnly { get; set; } = false;

        /// <summary>
        /// Limits selection scopes to the first N columns from each table based on database ordinal positions.
        /// </summary>
        public int? ColumnsPerTableLimit { get; set; } = 200;

        #endregion

        #region Initialization

        public ProjectionSettings(TableSelectPolicy rootSelectPolicy, TableSelectPolicy joinSelectPolicy, bool leftJoinsOnly, int? columnsPerTableLimit)
        {
            RootSelectPolicy = rootSelectPolicy;
            JoinSelectPolicy = joinSelectPolicy;
            LeftJoinsOnly = leftJoinsOnly;
            ColumnsPerTableLimit = columnsPerTableLimit;
        }

        #endregion
    }
}