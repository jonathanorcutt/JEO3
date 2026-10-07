using JEO3.Monitor;

namespace JEO3.Providers.Catalogs
{
    internal class SqliteQueryCatalog : ISqliteQueryCatalog
    {
        public string InitSqliteSchemaQuery => SQLiteQueries.InitSqliteSchemaQuery;
        public string PurgeHistoryAfter7Days => SQLiteQueries.PurgeHistoryAfter7Days;
    }
}
