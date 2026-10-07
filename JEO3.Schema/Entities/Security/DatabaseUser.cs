namespace JEO3.Schema
{
    public sealed class DatabaseUser : IDatabasePrincipal
    {
        public int PrincipalId { get; internal init; }
        public string? Name { get; internal init; }
        public string? TypeDescription { get; internal init; }
        public string? DefaultSchemaName { get; internal init; }
        public IReadOnlyList<IDatabaseRole> Roles { get; internal init; } = Array.Empty<IDatabaseRole>();
        public IReadOnlyList<IPrincipalPermission> Permissions { get; internal init; } = Array.Empty<IPrincipalPermission>();
    }
}
