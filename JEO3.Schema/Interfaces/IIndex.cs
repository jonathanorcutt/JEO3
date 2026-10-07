namespace JEO3.Schema
{
    public interface IIndex : IObject
    {
        long? IndexId { get; }
        IndexType IndexType { get; }
        string? ProviderIndexType { get; }
        bool IsUnique { get; }
        bool IsPrimaryKey { get; }
        bool IsUniqueConstraint { get; }
        bool IsDisabled { get; }
        bool IsFiltered { get; }
        string? FilterDefinition { get; }
        bool IsClustered { get; }
        bool IsCovering { get; }
        double? FragmentationPercentage { get; }
        long? PageCount { get; }
        ITable? Table { get; }

        IReadOnlyList<IIndexColumn> Columns { get; }

        IReadOnlyList<IColumn> KeyColumns { get; }

        IReadOnlyList<IColumn> IncludedColumns { get; }
    }
}
