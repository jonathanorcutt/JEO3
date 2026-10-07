namespace JEO3.Schema
{
    /// <summary>
    /// Represents the relationship between a column and an index.
    /// This is intentionally separate from IndexColumn so a column
    /// can expose its index participation without duplicating index
    /// metadata.
    /// </summary>
    public interface IColumnIndexLink
    {
        int? ObjectId { get; }
        long? ColumnId { get; }
        long? IndexId { get; }
        string? ObjectName { get; }
        string? ColumnName { get; }
        string? IndexName { get; }
        bool IsIncludedColumn { get; }
        int? OrdinalPosition { get; }
        int? IncludedOrdinal { get; }
        IndexColumnSortDirection SortDirection { get; }
        bool IsDescending { get; }
        bool IsIndexDisabled { get; }
        bool IsUnique { get; }
        bool IsPrimaryKey { get; }
        bool IsUniqueConstraint { get; }

        IColumn? Column { get; }

        IIndex? Index { get; }

        ITable? Table { get; }
    }
}
