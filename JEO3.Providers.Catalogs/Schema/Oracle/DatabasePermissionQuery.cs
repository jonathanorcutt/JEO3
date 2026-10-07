namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string DatabasePermissionQuery = @"
SELECT 
    perm.grantee_principal_id AS GranteePrincipalId,
    grantee.name AS GranteeName,
    perm.grantor_principal_id AS GrantorPrincipalId,
    grantor.name AS GrantorName,
    perm.state_desc AS StateDesc,
    perm.permission_name AS PermissionName,
    perm.major_id AS MajorId,
    perm.minor_id AS MinorId,
    perm.class_desc AS ClassDesc,
    CASE perm.class
        WHEN 0 THEN DB_NAME() -- DATABASE
        WHEN 3 THEN SCHEMA_NAME(perm.major_id) -- SCHEMA
        WHEN 1 THEN OBJECT_SCHEMA_NAME(perm.major_id) -- OBJECT_OR_COLUMN
        ELSE NULL
    END AS SecurableSchemaName,
    CASE perm.class
        WHEN 0 THEN NULL
        WHEN 3 THEN SCHEMA_NAME(perm.major_id)
        WHEN 1 THEN OBJECT_NAME(perm.major_id)
        ELSE CONVERT(VARCHAR(50), perm.major_id)
    END AS SecurableName
FROM sys.database_permissions perm
INNER JOIN sys.database_principals grantee ON perm.grantee_principal_id = grantee.principal_id
INNER JOIN sys.database_principals grantor ON perm.grantor_principal_id = grantor.principal_id;";

    }
}
