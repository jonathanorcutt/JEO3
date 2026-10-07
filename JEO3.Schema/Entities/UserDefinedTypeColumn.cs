namespace JEO3.Schema
{
    public sealed class UserDefinedTypeColumn : IUserDefinedTypeColumn
    {
        public int OrdinalPosition { get; internal init; }
        public string Name { get; internal init; }
        public string? DataType { get; internal init; }
        public string? ProviderDataType { get; internal init; }
        public int? MaximumLength { get; internal init; }
        public int? Precision { get; internal init; }
        public int? Scale { get; internal init; }
        public bool IsNullable { get; internal init; }
        public string DefaultValue { get; internal init; }
    }
}
