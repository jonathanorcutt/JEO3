namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string ViewDefinitionQuery = @"
SELECT 
    current_database() AS DatabaseName,
    v.oid::integer AS ObjectId,
    v.relname AS Name,
    s.nspname AS SchemaName,
    s.oid::integer AS SchemaId,
    NULL::timestamp AS CreateDate,
    NULL::timestamp AS ModifyDate,
    FALSE AS IsReplicated,
    
    -- Check if WITH CHECK OPTION is configured in reloptions array
    (
        EXISTS (
            SELECT 1 
            FROM unnest(v.reloptions) AS opt 
            WHERE opt LIKE 'check_option=%'
        )
    ) AS WithCheckOption,

    -- Reconstructs full CREATE OR REPLACE VIEW statement
    concat('CREATE OR REPLACE VIEW ""', s.nspname, '"".""', v.relname, '"" AS ', pg_get_viewdef(v.oid, true)) AS Definition

FROM pg_class v
JOIN pg_namespace s ON s.oid = v.relnamespace
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND v.relkind = 'v'; -- Filter specifically for views ('m' for materialized views)
";
    }
}
