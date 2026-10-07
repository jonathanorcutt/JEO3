namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string DatabasePrincipalQuery = @" 
SELECT 
    p.principal_id AS PrincipalId,
    p.name AS Name,
    p.type AS PrincipalType,
    p.type_desc AS TypeDescription,
    s.schema_id AS DefaultSchemaId,
    p.default_schema_name AS DefaultSchemaName,
    CAST(p.is_fixed_role AS BIT) AS IsFixedRole,
    p.owning_principal_id AS OwningPrincipalId
FROM sys.database_principals p
LEFT JOIN sys.schemas s ON p.default_schema_name = s.name
WHERE p.principal_id > 0;
";
    }
}
