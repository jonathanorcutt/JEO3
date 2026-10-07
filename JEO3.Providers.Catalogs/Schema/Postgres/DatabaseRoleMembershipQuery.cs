namespace JEO3.Providers.Catalogs
{
    internal static partial class PostgresQueries
    {
        internal const string DatabaseRoleMembershipQuery = @"
SELECT 
    m.roleid AS RolePrincipalId,
    role_p.rolname AS RoleName,
    m.member AS MemberPrincipalId,
    member_p.rolname AS MemberName
FROM pg_auth_members m
INNER JOIN pg_roles role_p ON m.roleid = role_p.oid
INNER JOIN pg_roles member_p ON m.member = member_p.oid;
";
    }
}
