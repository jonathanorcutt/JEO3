using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class ColumnIndexLink : IColumnIndexLink
    {
        [JsonInclude]
        public int? ObjectId { get; internal init; }
        [JsonInclude]
        public long? ColumnId { get; internal init; }
        [JsonInclude]
        public long? IndexId { get; internal init; }
        [JsonInclude]
        public string? ObjectName { get; internal init; }
        [JsonInclude]
        public string? ColumnName { get; internal init; }
        [JsonInclude]
        public string? IndexName { get; internal init; }
        [JsonInclude]
        public bool IsIncludedColumn { get; internal init; }
        [JsonInclude]
        public bool IsKeyColumn { get; init; }
        [JsonInclude]
        public int? OrdinalPosition { get; internal init; }
        [JsonInclude]
        public int? IncludedOrdinal { get; internal init; }
        [JsonInclude]
        public IndexColumnSortDirection SortDirection { get; internal init; }
        [JsonInclude]
        public bool IsDescending { get; internal init; }
        [JsonInclude]
        public bool IsIndexDisabled { get; internal init; }
        [JsonInclude]
        public bool IsUnique { get; internal init; }
        [JsonInclude]
        public bool IsPrimaryKey { get; internal init; }
        [JsonInclude]
        public bool IsUniqueConstraint { get; internal init; }
        // ============================================================
        // NAVIGATION
        // ============================================================

        public IColumn? Column { get; internal set; }

        public IIndex? Index { get; internal set; }

        public ITable? Table { get; internal set; }
    }
}