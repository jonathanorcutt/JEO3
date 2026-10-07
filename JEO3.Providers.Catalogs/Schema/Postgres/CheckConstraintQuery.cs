namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string CheckConstraintQuery = @"
SELECT 
    c.oid AS ObjectId,
    c.conrelid AS ParentObjectId,
    c.conkey[1] AS ParentColumnId,
    c.conname AS Name,
    pg_get_constraintdef(c.oid) AS Definition,
    NOT c.convalidated AS IsDisabled,
    NOT c.convalidated AS IsNotTrusted
FROM pg_constraint c
JOIN pg_namespace n ON n.oid = c.connamespace
WHERE c.contype = 'c'
  AND n.nspname NOT IN ('pg_catalog', 'information_schema');";
    }
}
