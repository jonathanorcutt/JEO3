namespace JEO3.Schema
{
    public interface IPrincipalPermission
    {
        string? PermissionName { get; }
        string? StateDescription { get; }
        string? ClassDescription { get; }
        string? SecurableName { get; }
    }
}
