namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string DatabasePrincipalQuery = @"
SELECT 
    p.PrincipalId,
    p.Name,
    p.PrincipalType,
    p.TypeDescription,
    s.SchemaId AS DefaultSchemaId,
    p.DefaultSchemaName,
    p.IsFixedRole,
    p.OwningPrincipalId
FROM (
    -- Database Users
    SELECT 
        u.user_id AS PrincipalId,
        u.username AS Name,
        'S' AS PrincipalType,
        'SQL_USER' AS TypeDescription,
        u.default_tablespace AS DefaultSchemaName,
        0 AS IsFixedRole,
        0 AS OwningPrincipalId
    FROM dba_users u

    UNION ALL

    -- Database Roles
    SELECT 
        r.role_id AS PrincipalId,
        r.role AS Name,
        'R' AS PrincipalType,
        'DATABASE_ROLE' AS TypeDescription,
        NULL AS DefaultSchemaName,
        CASE 
            WHEN r.role IN (
                'CONNECT', 'RESOURCE', 'DBA', 'SELECT_CATALOG_ROLE', 
                'EXECUTE_CATALOG_ROLE', 'DELETE_CATALOG_ROLE', 'EXP_FULL_DATABASE', 
                'IMP_FULL_DATABASE', 'LOGSTDBY_ADMINISTRATOR', 'DBFS_ROLE'
            ) THEN 1 
            ELSE 0 
        END AS IsFixedRole,
        0 AS OwningPrincipalId
    FROM dba_roles r
) p
LEFT JOIN (
    -- In Oracle, schemas map to users with non-null IDs
    SELECT user_id AS SchemaId, username AS SchemaName 
    FROM dba_users
) s ON p.DefaultSchemaName = s.SchemaName
ORDER BY p.PrincipalId
";
    }
}
