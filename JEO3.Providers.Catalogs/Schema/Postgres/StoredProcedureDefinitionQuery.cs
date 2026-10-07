namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string StoredProcedureDefinitionQuery = @"
SELECT 
    current_database() AS DatabaseName,
    p.oid::integer AS ObjectId,
    p.proname AS Name,
    s.nspname AS SchemaName,
    s.oid::integer AS SchemaId,
    NULL::timestamp AS CreateDate,
    NULL::timestamp AS ModifyDate,
    FALSE AS IsAutoExecuted,
    FALSE AS IsExecutionReplicated,
    pg_get_functiondef(p.oid) AS Definition
FROM pg_proc p
JOIN pg_namespace s ON s.oid = p.pronamespace
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema')
  AND p.prokind = 'p';
";
    }
}