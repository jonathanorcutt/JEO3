namespace JEO3.Schema
{
    public interface IIndexColumn
    {
        int? ObjectId { get; }
        long? IndexId { get; }
        long? ColumnId { get; }
        string? IndexName { get; }
        string? ColumnName { get; }
        int Ordinal { get; }
        bool IsIncluded { get; }
        bool IsKeyColumn { get; }
        IndexColumnSortDirection SortDirection { get; }
        bool IsDescending { get; }

        IIndex? Index { get; }

        IColumn? Column { get; }
    }
}
