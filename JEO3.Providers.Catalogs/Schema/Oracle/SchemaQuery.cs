namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        public const string SchemaQuery = @"
SELECT
    u.USER_ID AS ObjectId,
    u.USERNAME AS Name,
    sys_context('USERENV', 'DB_NAME') AS DatabaseName,
    u.USERNAME AS PrincipalName
FROM all_users u
WHERE u.ORACLE_MAINTAINED = 'N'
ORDER BY u.USERNAME";
    }
}
