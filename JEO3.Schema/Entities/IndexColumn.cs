namespace JEO3.Schema
{
    public sealed class IndexColumn : IIndexColumn
    {
        public int? ObjectId { get; internal init; }
        public long? IndexId { get; internal init; }
        public long? ColumnId { get; internal init; }
        public string? IndexName { get; internal init; }
        public string? ColumnName { get; internal init; }
        public int Ordinal { get; internal init; }
        public bool IsIncluded { get; internal init; }
        public bool IsKeyColumn { get; internal init; }
        public IndexColumnSortDirection SortDirection { get; internal init; }
        public bool IsDescending { get; internal init; }
        // ============================================================
        // NAVIGATION
        // ============================================================

        public IIndex? Index { get; internal init; }

        public IColumn? Column { get; internal init; }
    }
}