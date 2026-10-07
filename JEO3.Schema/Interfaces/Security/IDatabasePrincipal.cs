namespace JEO3.Schema
{
    public interface IDatabasePrincipal
    {
        int PrincipalId { get; }
        string? Name { get; }
        string? TypeDescription { get; }
    }
}
