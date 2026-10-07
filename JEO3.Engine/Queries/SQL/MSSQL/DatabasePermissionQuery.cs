using JEO3.Engine.Entities;

namespace JEO3.Engine
{
    internal static partial class MSSQLQueries
    {
        internal const string DatabasePermissionQuery = @"
SELECT 
    perm.grantee_principal_id AS " + nameof(FlatDatabasePermission.GranteePrincipalId) + @",
    grantee.name AS " + nameof(FlatDatabasePermission.GranteeName) + @",
    perm.grantor_principal_id AS " + nameof(FlatDatabasePermission.GrantorPrincipalId) + @",
    grantor.name AS " + nameof(FlatDatabasePermission.GrantorName) + @",
    perm.state_desc AS " + nameof(FlatDatabasePermission.StateDesc) + @",
    perm.permission_name AS " + nameof(FlatDatabasePermission.PermissionName) + @",
    perm.major_id AS " + nameof(FlatDatabasePermission.MajorId) + @",
    perm.minor_id AS " + nameof(FlatDatabasePermission.MinorId) + @",
    perm.class_desc AS " + nameof(FlatDatabasePermission.ClassDesc) + @",
    CASE perm.class
        WHEN 0 THEN DB_NAME() -- DATABASE
        WHEN 3 THEN SCHEMA_NAME(perm.major_id) -- SCHEMA
        WHEN 1 THEN OBJECT_SCHEMA_NAME(perm.major_id) -- OBJECT_OR_COLUMN
        ELSE NULL
    END AS " + nameof(FlatDatabasePermission.SecurableSchemaName) + @",
    CASE perm.class
        WHEN 0 THEN NULL
        WHEN 3 THEN SCHEMA_NAME(perm.major_id)
        WHEN 1 THEN OBJECT_NAME(perm.major_id)
        ELSE CONVERT(VARCHAR(50), perm.major_id)
    END AS " + nameof(FlatDatabasePermission.SecurableName) + @"
FROM sys.database_permissions perm WITH (NOLOCK)
INNER JOIN sys.database_principals grantee WITH (NOLOCK) ON perm.grantee_principal_id = grantee.principal_id
INNER JOIN sys.database_principals grantor WITH (NOLOCK) ON perm.grantor_principal_id = grantor.principal_id;";

    }
}
