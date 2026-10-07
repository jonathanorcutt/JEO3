namespace JEO3.Providers.Catalogs
{
    internal static partial class MSSQLQueries
    {
        public const string SchemaQuery = @"
SELECT
s.schema_id AS ObjectId,
s.name AS Name,
DB_NAME() AS DatabaseName,
p.name AS PrincipalName
FROM sys.schemas s
JOIN sys.database_principals p ON p.principal_id = s.principal_id
WHERE
    s.name not in ('sys', 'guest', 'INFORMATION_SCHEMA', 'db_accessadmin','db_backupoperator','db_datareader','db_datawriter','db_ddladmin','db_denydatareader','db_denydatawriter','db_owner','db_securityadmin')
ORDER BY name
";
    }
}
