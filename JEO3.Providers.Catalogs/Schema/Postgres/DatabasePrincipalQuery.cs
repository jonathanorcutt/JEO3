namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string DatabasePrincipalQuery = @"
SELECT 
    r.oid AS PrincipalId,
    r.rolname AS Name,
    CASE 
        WHEN r.rolcanlogin THEN 'S'  -- SQL User
        ELSE 'R'                     -- Database Role
    END AS PrincipalType,
    CASE 
        WHEN r.rolcanlogin THEN 'SQL_USER'
        ELSE 'DATABASE_ROLE'
    END AS TypeDescription,
    s.oid::integer AS DefaultSchemaId,
    COALESCE(
        -- Extracts search_path setting if explicitly assigned to the role
        (SELECT substring(unnest(r.rolconfig) from 'search_path=([^, ]+)')::text),
        'public'
    ) AS DefaultSchemaName,
    (r.rolsuper OR r.rolreplication OR r.rolcreatedb OR r.rolcreaterole) AS IsFixedRole,
    NULL::integer AS OwningPrincipalId
FROM pg_roles r
LEFT JOIN pg_namespace s ON s.nspname = COALESCE(
    (SELECT substring(unnest(r.rolconfig) from 'search_path=([^, ]+)')::text),
    'public'
);
";
    }
}
