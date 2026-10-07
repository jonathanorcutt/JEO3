namespace JEO3.Schema
{
    public sealed class RelationColumnPair : IRelationColumnPair
    {
        // JEO
        public bool IsComposite { get; internal init; }
        public bool IsNullable { get; internal init; }

        public long? RelationId { get; internal init; }
        public int Ordinal { get; internal init; }
        public long? ParentColumnId { get; internal init; }
        public long? ReferencedColumnId { get; internal init; }
        public string? ParentColumnName { get; internal init; }
        public string? ReferencedColumnName { get; internal init; }
        public string? ParentTableName { get; internal init; }
        public string? ReferencedTableName { get; internal init; }

        // ============================================================
        // NAVIGATION
        // ============================================================

        public IColumn? ParentColumn { get; set; }

        public IColumn? ReferencedColumn { get; set; }

        public IForeignKey? ForeignKey { get; internal set; }
    }
}