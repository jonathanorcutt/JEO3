namespace JEO3.Engine
{
    internal static partial class PostgresQueries
    {
        internal const string TableQuery = @"
SELECT
    current_database() AS DatabaseName,
    n.nspname AS SchemaName,
    c.relname AS TableName,
    CASE 
        WHEN n.nspname = 'public' THEN c.relname 
        ELSE n.nspname || '.' || c.relname 
    END AS TableFullName,
    COALESCE(c.reltuples::bigint, 0) AS Rows
FROM pg_class c
    INNER JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE 
    c.relkind = 'r' -- Only ordinary tables
    AND n.nspname NOT IN ('pg_catalog', 'information_schema') -- Filter out system tables
ORDER BY
    n.nspname,
    c.relname;";
    }
}