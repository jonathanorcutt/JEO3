namespace JEO3.Schema
{
    public interface ICheckConstraint
    {
        long? DatabaseId { get; }
        string DatabaseName { get; }
        int TableObjectId { get; }
        string SchemaName { get; }
        string TableName { get; }
        string ColumnName { get; }
        int ObjectId { get; }
        int ParentObjectId { get; }
        int ParentColumnId { get; }
        string Name { get; }
        string Definition { get; }
        bool IsDisabled { get; }
        bool IsNotTrusted { get; }
        string Path { get; }
        public string? IssueDescription { get; }
        public string? RemediationScript { get; }
    }
}
