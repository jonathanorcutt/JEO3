namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string SynonymQuery = @"
SELECT 
    v.oid::integer AS Id,
    s.oid::integer AS SchemaId,
    s.nspname AS SchemaName,
    v.relname AS Name,
    
    -- Extracted Target Schema (parses view definition or defaults to current schema)
    COALESCE(
        (SELECT (regexp_matches(pg_get_viewdef(v.oid), 'FROM\s+""?([a-zA-Z0-9_]+)""?\.""?([a-zA-Z0-9_]+)""?', 'i'))[1]),
        s.nspname
    ) AS TargetSchemaName,

    -- Extracted Target Table Name
    COALESCE(
        (SELECT (regexp_matches(pg_get_viewdef(v.oid), 'FROM\s+""?([a-zA-Z0-9_]+)""?\.""?([a-zA-Z0-9_]+)""?', 'i'))[2]),
        v.relname
    ) AS TargetTableName,

    -- Full Target Base Object Name
    concat(s.nspname, '.', v.relname) AS BaseObjectName,
    v.relowner::integer AS PrincipalId,
    0 AS ParentObjectId,
    'SN' AS Type,
    'SYNONYM' AS TypeDesc,
    NULL::timestamp AS CreateDate,
    NULL::timestamp AS ModifyDate,
    FALSE AS IsMsShipped,
    FALSE AS IsPublished,
    FALSE AS IsSchemaPublished

FROM pg_class v
JOIN pg_namespace s ON s.oid = v.relnamespace
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND v.relkind = 'v'; -- Views representing alias/synonym mappings
";
    }
}