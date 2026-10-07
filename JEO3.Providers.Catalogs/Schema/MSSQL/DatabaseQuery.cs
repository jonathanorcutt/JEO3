namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        public const string DatabaseQuery = @"
SELECT
    database_id AS ObjectId,
    name AS Name,
    name AS DatabaseName,
    compatibility_level AS CompatibilityLevel,
    is_read_committed_snapshot_on AS IsReadCommitted,
    create_date AS CreateDate,
    collation_name AS CollationName
FROM sys.databases
WHERE name = DB_NAME();";
    }
}
