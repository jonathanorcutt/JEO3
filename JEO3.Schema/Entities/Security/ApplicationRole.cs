namespace JEO3.Schema
{
    public sealed class ApplicationRole : IDatabasePrincipal
    {
        public int PrincipalId { get; internal init; }
        public string? Name { get; internal init; }
        public string? TypeDescription { get; internal init; }
        public IReadOnlyList<IPrincipalPermission> Permissions { get; internal init; } = Array.Empty<IPrincipalPermission>();
    }
}
