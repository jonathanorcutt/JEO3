namespace JEO3.Generation.Models
{
    /// <summary>
    /// A pure, engine-level DTO that specifies whether a database foreign key relationship 
    /// should be actively traversed or explicitly bypassed during query generation.
    /// </summary>
    public sealed class GraphRelationConstraint
    {
        #region Propeties

        public required string KeyName { get; init; }
        public required string PrimaryTable { get; init; }
        public required string PrimaryColumns { get; init; }
        public required string ForeignTable { get; init; }
        public required string ForeignColumns { get; init; }
        public required string Signature { get; init; }

        /// <summary>
        /// Indicates if this relationship was manually forged by the user in the UI 
        /// rather than pulled from physical database metadata.
        /// </summary>
        public bool IsUserDefined { get; set; } = true;

        // TODO: Implement
        public bool IsDisabled { get; set; }

        #endregion

        #region Initialization

        public GraphRelationConstraint() { }

        #endregion

        #region Functions

        /// <summary>
        /// Creates an engine-level relationship constraint forged manually by the user in the UI.
        /// </summary>
        public static GraphRelationConstraint CreateManual(
            string primaryTable,
            string primaryColumn,
            string foreignTable,
            string foreignColumn,
            string signature,
            bool isDisabled = true,
            bool isUserDefined = true)
        {
            return new GraphRelationConstraint
            {
                KeyName = $"FK_MANUAL_{foreignTable}_{primaryTable}_{foreignColumn}",
                PrimaryTable = primaryTable,
                PrimaryColumns = primaryColumn,
                ForeignTable = foreignTable,
                ForeignColumns = foreignColumn,
                Signature = signature,
                IsUserDefined = isUserDefined,
                IsDisabled = isDisabled
            };
        }

        #endregion
    }
}