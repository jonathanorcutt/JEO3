namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string ExtendedPropertyQuery = @"
SELECT 
    d.objoid::integer AS MajorId,
    d.objsubid::integer AS MinorId,
    CASE 
        WHEN d.objsubid > 0 THEN 1 -- Column
        WHEN c.relkind = 'i' THEN 7 -- Index
        ELSE 1                      -- Table / Other Object
    END AS Class,
    CASE 
        WHEN d.objsubid > 0 THEN 'OBJECT_OR_COLUMN'
        WHEN c.relkind = 'i' THEN 'INDEX'
        ELSE 'OBJECT_OR_COLUMN'
    END AS ClassDescription,
    'MS_Description' AS PropertyName,
    d.description AS Value,
    s.nspname AS SchemaName,
    c.oid::integer AS ObjectId,
    c.relname AS Name,
    CASE c.relkind
        WHEN 'r' THEN 'USER_TABLE'
        WHEN 'v' THEN 'VIEW'
        WHEN 'm' THEN 'MATERIALIZED_VIEW'
        WHEN 'i' THEN 'INDEX'
        WHEN 'p' THEN 'PARTITIONED_TABLE'
        ELSE 'OTHER'
    END AS ObjectTypeDescription,
    
    -- Identify the Parent Object (Table holding Column, Index, or Constraint)
    CASE 
        WHEN d.objsubid > 0 THEN c.oid::integer          -- Parent table for column
        WHEN c.relkind = 'i' THEN i.indrelid::integer    -- Parent table for index
        ELSE c.oid::integer
    END AS ParentObjectId,
    
    CASE 
        WHEN d.objsubid > 0 THEN c.relname
        WHEN c.relkind = 'i' THEN parent_tab.relname
        ELSE c.relname
    END AS ParentObjectName,
    
    col.attname AS ColumnName,
    CASE WHEN c.relkind = 'i' THEN c.relname ELSE NULL END AS IndexName

FROM pg_description d
JOIN pg_class c ON c.oid = d.objoid
JOIN pg_namespace s ON s.oid = c.relnamespace
LEFT JOIN pg_attribute col ON col.attrelid = d.objoid AND col.attnum = d.objsubid AND d.objsubid > 0
LEFT JOIN pg_index i ON i.indexrelid = c.oid AND c.relkind = 'i'
LEFT JOIN pg_class parent_tab ON parent_tab.oid = i.indrelid

WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND c.relkind IN ('r', 'v', 'm', 'i', 'p');
";
    }
}