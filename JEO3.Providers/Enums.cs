namespace JEO3.Providers
{
    public enum DatabaseProviderType
    {
        Unknown,
        MSSQL,
        Postgres,
        Oracle,
        SQLite
    }

    public enum CommandType
    {
        Text,
        StoredProcedure
    }
}
