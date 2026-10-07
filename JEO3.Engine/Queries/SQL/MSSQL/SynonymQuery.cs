namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string SynonymQuery = @"
SELECT 
    s.object_id AS Id,
    s.schema_id AS SchemaId,
    sch.name AS SchemaName,
    s.name AS Name,
    -- Database cleanly extracts the targets for C# mapping
    COALESCE(PARSENAME(s.base_object_name, 2), 'dbo') AS TargetSchemaName,
    PARSENAME(s.base_object_name, 1) AS TargetTableName,
    s.base_object_name AS BaseObjectName,
    s.principal_id AS PrincipalId,
    s.parent_object_id AS ParentObjectId,
    s.type AS Type,
    s.type_desc AS TypeDesc,
    s.create_date AS CreateDate,
    s.modify_date AS ModifyDate,
    s.is_ms_shipped AS IsMsShipped,
    s.is_published AS IsPublished,
    s.is_schema_published AS IsSchemaPublished
FROM sys.synonyms s WITH (NOLOCK)
INNER JOIN sys.schemas sch WITH (NOLOCK) ON s.schema_id = sch.schema_id;";
    }
}
