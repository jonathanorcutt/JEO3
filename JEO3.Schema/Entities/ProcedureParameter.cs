namespace JEO3.Schema
{
    public sealed class ProcedureParameter : IProcedureParameter
    {
        public int? ObjectId { get; internal init; }
        public int Ordinal { get; internal init; }
        public string Name { get; internal init; } = string.Empty;
        public string? DataType { get; internal init; }
        public string? ProviderDataType { get; internal init; }
        public int? MaxLength { get; internal init; }
        public int? Precision { get; internal init; }
        public int? Scale { get; internal init; }
        public bool IsOutput { get; internal init; }
        public bool IsNullable { get; internal init; }
        public string? DefaultValue { get; internal init; }

        public string? UserDefinedTypeSchema { get; internal init; }
        public string? UserDefinedTypeName { get; internal init; }
    }
}