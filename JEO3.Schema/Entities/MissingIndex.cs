using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class MissingIndex : ObjectBase, IMissingIndex
    {
        [JsonInclude]
        public override DatabaseObjectType ObjectType => DatabaseObjectType.MissingIndex;
        [JsonInclude]
        public string TablePath { get; internal init; } = string.Empty;
        [JsonInclude]
        public int? ParentObjectId { get; internal init; }
        [JsonInclude]
        public long? DatabaseId { get; internal init; }
        [JsonInclude]
        public string? TableName { get; internal init; }
        [JsonInclude]
        public long? IndexGroupHandle { get; internal init; }
        [JsonInclude]
        public long? IndexHandle { get; internal init; }
        [JsonInclude]
        public long? UserSeeks { get; internal init; }
        [JsonInclude]
        public long? UserScans { get; internal init; }
        [JsonInclude]
        public long? UserReads { get; internal init; }
        [JsonInclude]
        public decimal? AvgUserImpact { get; internal init; }
        [JsonInclude]
        public decimal? AvgTotalUserCost { get; internal init; }
        [JsonInclude]
        public DateTime? LastUserSeek { get; internal init; }
        [JsonInclude]
        public decimal? ImprovementMeasure { get; internal init; }
        [JsonInclude]
        public string? EqualityColumnsDisplayString { get; internal init; }
        [JsonInclude]
        public string? InequalityColumnsDisplayString { get; internal init; }
        [JsonInclude]
        public string? IncludedColumnsDisplayString { get; internal init; }
        [JsonInclude]
        public string? SuggestedKeyColumnsDisplayString { get; internal init; }
        [JsonInclude]
        public string? EqualityColumnsString { get; internal init; }
        [JsonInclude]
        public string? InequalityColumnsString { get; internal init; }
        [JsonInclude]
        public string? IncludedColumnsString { get; internal init; }
        [JsonInclude]
        public string? SuggestedKeyColumnsString { get; internal init; }


        [JsonInclude]
        public string? IndexCreationScript { get; internal init; }


        [JsonInclude]
        public IReadOnlyList<IColumn> EqualityColumns { get; internal set; }
        [JsonInclude]
        public IReadOnlyList<IColumn> InequalityColumns { get; internal set; }
        [JsonInclude]
        public IReadOnlyList<IColumn> IncludedColumns { get; internal set; }
        [JsonInclude]
        public IReadOnlyList<IColumn> SuggestedKeyColumns { get; internal set; }
        [JsonInclude]
        public decimal Score => (UserSeeks ?? 0m) * (AvgTotalUserCost ?? 0m) * (AvgUserImpact ?? 0m) / 100m;

        // Canonical navigation
        public ITable? Table { get; internal set; }
        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}
