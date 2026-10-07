namespace JEO3.Code
{
    public record GeneratedFile(string ClassName, string SchemaName, string FileContent);
    public sealed record GeneratedClassCode(string Namespace, string ClassName, IReadOnlyList<string> UsingNamespaces, string Code);
    public sealed record GeneratedNamespaceCode(string Namespace, IReadOnlyList<string> UsingNamespaces, IReadOnlyList<GeneratedClassCode> Classes);
}
