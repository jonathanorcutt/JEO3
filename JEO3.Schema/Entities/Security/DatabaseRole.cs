namespace JEO3.Schema
{
    public sealed class DatabaseRole : IDatabaseRole
    {
        public int PrincipalId { get; internal init; }
        public string? Name { get; internal init; }
        public string? TypeDescription { get; internal init; }
        public bool IsFixedRole { get; internal init; }
        public IReadOnlyList<IDatabasePrincipal> Members { get; internal set; } = Array.Empty<IDatabasePrincipal>();
        public IReadOnlyList<IPrincipalPermission> Permissions { get; internal init; } = Array.Empty<IPrincipalPermission>();
    }

}
