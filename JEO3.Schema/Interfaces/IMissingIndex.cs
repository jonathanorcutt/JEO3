namespace JEO3.Schema
{
    public interface IMissingIndex : IObject
    {
        long? DatabaseId { get; }
        string? TableName { get; }
        long? IndexGroupHandle { get; }
        long? IndexHandle { get; }
        long? UserSeeks { get; }
        long? UserScans { get; }
        long? UserReads { get; }
        decimal? AvgUserImpact { get; }
        decimal? AvgTotalUserCost { get; }
        DateTime? LastUserSeek { get; }
        decimal? ImprovementMeasure { get; }
        string? EqualityColumnsDisplayString { get; }
        string? InequalityColumnsDisplayString { get; }
        string? IncludedColumnsDisplayString { get; }
        string? SuggestedKeyColumnsDisplayString { get; }
        string? EqualityColumnsString { get; }
        string? InequalityColumnsString { get; }
        string? IncludedColumnsString { get; }
        string? SuggestedKeyColumnsString { get; }
        IReadOnlyList<IColumn> EqualityColumns { get; }
        IReadOnlyList<IColumn> InequalityColumns { get; }
        IReadOnlyList<IColumn> IncludedColumns { get; }
        IReadOnlyList<IColumn> SuggestedKeyColumns { get; }
        string? IndexCreationScript { get; }
        decimal Score { get; }

        ITable? Table { get; }
    }
}
