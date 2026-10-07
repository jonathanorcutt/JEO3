namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        public const string SchemaQuery = @"
SELECT
    s.oid::integer AS ObjectId,
    s.nspname AS Name,
    current_database() AS DatabaseName,
    p.rolname AS PrincipalName
FROM pg_namespace s
JOIN pg_roles p ON p.oid = s.nspowner
WHERE s.nspname NOT IN ('pg_catalog', 'information_schema', 'pg_toast')
  AND s.nspname NOT LIKE 'pg_temp_%'
  AND s.nspname NOT LIKE 'pg_toast_temp_%'
ORDER BY s.nspname;
";
    }
}
