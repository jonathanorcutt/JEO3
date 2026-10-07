namespace JEO3.Schema
{
    public sealed class Function : ObjectBase, IFunction
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Function;
        public FunctionType FunctionType { get; }
        public string? Definition { get; internal init; }
        public string? ReturnType { get; internal init; }
        public string? ProviderReturnType { get; internal init; }
        public bool IsSystemObject { get; internal init; }
        public bool IsDeterministic { get; internal init; }
        public IReadOnlyList<IProcedureParameter> Parameters { get; internal set; } = Array.Empty<IProcedureParameter>();
        public IReadOnlyList<IColumn> ReturnColumns { get; internal set; } = Array.Empty<IColumn>();

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}