namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        // [FIX] Was an empty string. Now queries sys.extended_properties
        // and matches FlatExtendedProperty 1:1 (MajorId, MinorId, Class,
        // ClassDescription, Name, Value).
        internal const string ExtendedPropertyQuery = @"
SELECT
    ep.major_id AS MajorId,
    ep.minor_id AS MinorId,
    ep.class AS Class,
    ep.class_desc AS ClassDescription,
    ep.name AS PropertyName,
    CAST(ep.value AS nvarchar(max)) AS Value,
    s.name AS SchemaName,
    o.object_id AS ObjectId,
    o.name AS Name,
    --o.type AS ObjectType,
    o.type_desc AS ObjectTypeDescription,
    -- Identify the Parent Object (e.g., the Table holding a Column, Index, or Constraint)
    CASE 
        WHEN ep.class = 1 AND ep.minor_id > 0 THEN o.object_id 
        ELSE o.parent_object_id 
    END AS ParentObjectId,
    OBJECT_NAME(CASE 
        WHEN ep.class = 1 AND ep.minor_id > 0 THEN o.object_id 
        ELSE o.parent_object_id 
    END) AS ParentObjectName,
    c.name AS ColumnName,
    i.name AS IndexName
FROM sys.extended_properties ep WITH (NOLOCK)
-- Resolve objects for Class 1 (Tables, Views, Columns, Constraints) and Class 7 (Indexes)
LEFT JOIN sys.all_objects o WITH (NOLOCK) ON ep.major_id = o.object_id AND ep.class IN (1, 7)
LEFT JOIN sys.schemas s WITH (NOLOCK) ON o.schema_id = s.schema_id
-- Join to isolate column-level metadata
LEFT JOIN sys.columns c WITH (NOLOCK) ON ep.class = 1 AND ep.major_id = c.object_id AND ep.minor_id = c.column_id
-- Join to isolate index-level metadata (Class 7)
LEFT JOIN sys.indexes i WITH (NOLOCK) ON ep.class = 7 AND ep.major_id = i.object_id AND ep.minor_id = i.index_id
WHERE ep.class_desc NOT IN ('DATABASE', 'SCHEMA');";
    }
}
