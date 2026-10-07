namespace JEO3.Providers.Catalogs
{
    public interface ISqliteQueryCatalog
    {
        string InitSqliteSchemaQuery { get; }
        string PurgeHistoryAfter7Days { get; }
    }
}
