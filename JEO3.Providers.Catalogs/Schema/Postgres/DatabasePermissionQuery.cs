namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string DatabasePermissionQuery = @"
WITH all_permissions AS (
    -- 1. Database Level Permissions
    SELECT 
        grantee_role.oid AS GranteePrincipalId,
        p.grantee AS GranteeName,
        grantor_role.oid AS GrantorPrincipalId,
        p.grantor AS GrantorName,
        'GRANT' AS StateDesc,
        p.privilege_type AS PermissionName,
        0 AS MajorId,
        0 AS MinorId,
        'DATABASE' AS ClassDesc,
        current_database() AS SecurableSchemaName,
        current_database() AS SecurableName
    FROM (
        SELECT 
            (aclexplode(db.datacl)).grantor AS grantor_oid,
            (aclexplode(db.datacl)).grantee AS grantee_oid,
            (aclexplode(db.datacl)).privilege_type AS privilege_type
        FROM pg_database db
        WHERE db.datname = current_database()
          AND db.datacl IS NOT NULL
    ) raw_acl
    JOIN pg_roles grantee_role ON grantee_role.oid = raw_acl.grantee_oid
    JOIN pg_roles grantor_role ON grantor_role.oid = raw_acl.grantor_oid
    CROSS JOIN (SELECT current_database() AS dbname) d
    JOIN (
        SELECT 
            grantor, grantee, privilege_type 
        FROM information_schema.usage_privileges 
        WHERE object_type = 'DATABASE'
    ) p ON p.grantee = grantee_role.rolname AND p.grantor = grantor_role.rolname

    UNION ALL

    -- 2. Schema Level Permissions
    SELECT 
        grantee_role.oid AS GranteePrincipalId,
        p.grantee AS GranteeName,
        grantor_role.oid AS GrantorPrincipalId,
        p.grantor AS GrantorName,
        'GRANT' AS StateDesc,
        p.privilege_type AS PermissionName,
        s.oid::integer AS MajorId,
        0 AS MinorId,
        'SCHEMA' AS ClassDesc,
        p.object_name AS SecurableSchemaName,
        p.object_name AS SecurableName
    FROM information_schema.usage_privileges p
    JOIN pg_namespace s ON s.nspname = p.object_name
    LEFT JOIN pg_roles grantee_role ON grantee_role.rolname = p.grantee
    LEFT JOIN pg_roles grantor_role ON grantor_role.rolname = p.grantor
    WHERE p.object_type = 'SCHEMA'

    UNION ALL

    -- 3. Table / Object Level Permissions
    SELECT 
        grantee_role.oid AS GranteePrincipalId,
        p.grantee AS GranteeName,
        grantor_role.oid AS GrantorPrincipalId,
        p.grantor AS GrantorName,
        'GRANT' AS StateDesc,
        p.privilege_type AS PermissionName,
        t.oid::integer AS MajorId,
        0 AS MinorId,
        'OBJECT_OR_COLUMN' AS ClassDesc,
        p.table_schema AS SecurableSchemaName,
        p.table_name AS SecurableName
    FROM information_schema.table_privileges p
    JOIN pg_class t ON t.relname = p.table_name
    JOIN pg_namespace s ON s.oid = t.relnamespace AND s.nspname = p.table_schema
    LEFT JOIN pg_roles grantee_role ON grantee_role.rolname = p.grantee
    LEFT JOIN pg_roles grantor_role ON grantor_role.rolname = p.grantor

    UNION ALL

    -- 4. Column Level Permissions
    SELECT 
        grantee_role.oid AS GranteePrincipalId,
        p.grantee AS GranteeName,
        grantor_role.oid AS GrantorPrincipalId,
        p.grantor AS GrantorName,
        'GRANT' AS StateDesc,
        p.privilege_type AS PermissionName,
        t.oid::integer AS MajorId,
        c.attnum::integer AS MinorId,
        'OBJECT_OR_COLUMN' AS ClassDesc,
        p.table_schema AS SecurableSchemaName,
        p.table_name AS SecurableName
    FROM information_schema.column_privileges p
    JOIN pg_class t ON t.relname = p.table_name
    JOIN pg_namespace s ON s.oid = t.relnamespace AND s.nspname = p.table_schema
    JOIN pg_attribute c ON c.attrelid = t.oid AND c.attname = p.column_name
    LEFT JOIN pg_roles grantee_role ON grantee_role.rolname = p.grantee
    LEFT JOIN pg_roles grantor_role ON grantor_role.rolname = p.grantor
)
SELECT 
    GranteePrincipalId,
    GranteeName,
    GrantorPrincipalId,
    GrantorName,
    StateDesc,
    PermissionName,
    MajorId,
    MinorId,
    ClassDesc,
    SecurableSchemaName,
    SecurableName
FROM all_permissions;
";
    }
}
