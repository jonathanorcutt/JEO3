namespace JEO3.Schema
{
    public interface IProcedure : IObject
    {
        string? Definition { get; }
        bool IsSystemObject { get; }
        bool IsEncrypted { get; }
        IReadOnlyList<IProcedureParameter> Parameters { get; }
        List<DescribeFirstResultSet> OutputColumns { get; set; }
        string OutputCSharpDefinition { get; set; }
    }
}
