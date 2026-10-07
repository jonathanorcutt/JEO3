namespace JEO3.Schema
{
    public interface IProcedureParameter
    {
        int? ObjectId { get; }
        int Ordinal { get; }
        string Name { get; }
        string? DataType { get; }
        string? ProviderDataType { get; }
        int? MaxLength { get; }
        int? Precision { get; }
        int? Scale { get; }
        bool IsOutput { get; }
        bool IsNullable { get; }
        string? DefaultValue { get; }
    }
}
