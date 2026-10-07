namespace JEO3.Schema
{
    public interface IUserDefinedTypeColumn
    {
        int OrdinalPosition { get; }
        string Name { get; }
        string? DataType { get; }
        string? ProviderDataType { get; }
        int? MaximumLength { get; }
        int? Precision { get; }
        int? Scale { get; }
        bool IsNullable { get; }
        string DefaultValue { get; }
    }
}
