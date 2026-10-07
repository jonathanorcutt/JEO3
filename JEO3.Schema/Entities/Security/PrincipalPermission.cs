namespace JEO3.Schema
{

    public sealed class PrincipalPermission : IPrincipalPermission
    {
        public string? PermissionName { get; internal init; }
        public string? StateDescription { get; internal init; }
        public string? ClassDescription { get; internal init; }
        public string? SecurableSchemaName { get; internal init; }
        public string? SecurableName { get; internal init; }
    }
}
