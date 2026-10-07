using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class CheckConstraint : ICheckConstraint
    {
        //public override DatabaseObjectType ObjectType => DatabaseObjectType.Constraint;

        [JsonInclude]
        public int TableObjectId { get; internal init; }
        [JsonInclude]
        public long? DatabaseId { get; internal init; }
        [JsonInclude]
        public string DatabaseName { get; internal init; }
        [JsonInclude]
        public string SchemaName { get; internal init; }
        [JsonInclude]
        public string TableName { get; internal init; }
        [JsonInclude]
        public string ColumnName { get; internal init; }
        public string? ColumnObjectId => $"{ParentObjectId}_{ParentColumnId}";
        [JsonInclude]
        public int ObjectId { get; internal init; }
        [JsonInclude]
        public int ParentObjectId { get; internal init; }
        [JsonInclude]
        public int ParentColumnId { get; internal init; }
        [JsonInclude]
        public string Name { get; internal init; } = string.Empty;
        [JsonInclude]
        public string Definition { get; internal init; } = string.Empty;
        [JsonInclude]
        public bool IsDisabled { get; internal init; }
        [JsonInclude]
        public bool IsNotTrusted { get; internal init; }
        [JsonInclude]
        public string Path { get; internal init; }
        [JsonInclude]
        public string? IssueDescription { get; internal init; }
        [JsonInclude]
        public string? RemediationScript { get; internal init; }
    }
}
