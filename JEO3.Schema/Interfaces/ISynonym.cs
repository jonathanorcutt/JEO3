namespace JEO3.Schema
{
    public interface ISynonym
    {
        int Id { get; }
        int SchemaId { get; }
        string SchemaName { get; }
        string Name { get; }
        string TargetSchemaName { get; }
        string TargetTableName { get; }
        string BaseObjectName { get; }
        int? PrincipalId { get; }
        int ParentObjectId { get; }
        string Type { get; }
        string TypeDesc { get; }
        DateTime CreateDate { get; }
        DateTime ModifyDate { get; }
        bool IsMsShipped { get; }
        bool IsPublished { get; }
        bool IsSchemaPublished { get; }
        Table? TargetTable { get; }
    }
}