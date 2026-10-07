namespace JEO3.Providers.Catalogs
{
    internal static partial class OracleQueries
    {
        internal const string DatabaseRoleMembershipQuery = @"SELECT 
    rm.role_principal_id AS RolePrincipalId,
    role_p.name AS RoleName,
    rm.member_principal_id AS MemberPrincipalId,
    member_p.name AS MemberName
FROM sys.database_role_members rm
INNER JOIN sys.database_principals role_p ON rm.role_principal_id = role_p.principal_id
INNER JOIN sys.database_principals member_p ON rm.member_principal_id = member_p.principal_id;";

    }
}
