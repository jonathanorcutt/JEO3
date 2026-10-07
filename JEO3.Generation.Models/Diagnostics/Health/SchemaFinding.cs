namespace JEO3.Generation.Models
{
    public sealed class SchemaFinding
    {
        public SchemaObjectType ObjectType { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public SchemaFindingSeverity Severity { get; init; }
        public IssueType? IssueType { get; init; }
        public string? Recommendation { get; init; }
        public string? DatabaseName { get; init; }
        public string? SchemaName { get; init; }
        public string? ObjectName { get; init; }
        public string? ObjectPath { get; init; }
        public string? ColumnName { get; init; }
        public string? ForeignKeyName { get; init; }
        public string? IndexName { get; init; }
        public int Score { get; init; }
    }
}
