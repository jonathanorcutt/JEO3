namespace JEO3.Engine
{
    internal partial class PostgresQueries
    {
        internal const string DatabasePrincipalQuery = @"SELECT 
    p.principal_id AS PrincipalId,
    p.name AS Name,
    p.type AS PrincipalType,
    p.type_desc AS TypeDescription,
    p.default_schema_id AS DefaultSchemaId,
    s.name AS DefaultSchemaName,
    CAST(p.is_fixed_role AS BIT) AS IsFixedRole,
    p.owning_principal_id AS OwningPrincipalId
FROM sys.database_principals p
LEFT JOIN sys.schemas s ON p.default_schema_id = s.schema_id
WHERE p.principal_id > 0; -- Exclude public or internal system artifacts if needed
";
    }
}
