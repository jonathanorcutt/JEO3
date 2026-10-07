namespace JEO3.Schema
{
    public interface IDatabaseRole : IDatabasePrincipal
    {
        bool IsFixedRole { get; }
        IReadOnlyList<IDatabasePrincipal> Members { get; }
        IReadOnlyList<IPrincipalPermission> Permissions { get; }
    }
}
