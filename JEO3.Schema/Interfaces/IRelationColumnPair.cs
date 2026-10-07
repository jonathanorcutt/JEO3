namespace JEO3.Schema
{
    public interface IRelationColumnPair
    {
        // JEO
        bool IsComposite { get; }
        bool IsNullable { get; }

        long? RelationId { get; }
        int Ordinal { get; }
        long? ParentColumnId { get; }
        long? ReferencedColumnId { get; }
        string? ParentColumnName { get; }
        string? ReferencedColumnName { get; }
        string? ParentTableName { get; }
        string? ReferencedTableName { get; }

        IColumn? ParentColumn { get; set; }

        IColumn? ReferencedColumn { get; set; }

        IForeignKey? ForeignKey { get; }
    }
}
