namespace JEO3.Schema
{
    public sealed class Procedure : ObjectBase, IProcedure
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Procedure;
        public string? Definition { get; internal init; }
        public bool IsSystemObject { get; internal init; }
        public bool IsEncrypted { get; internal init; }
        public IReadOnlyList<IProcedureParameter> Parameters { get; internal set; } = Array.Empty<IProcedureParameter>();
        public List<DescribeFirstResultSet> OutputColumns { get; set; } = [];
        public string OutputCSharpDefinition { get; set; } = string.Empty;

        public override IObject? Parent { get; internal set; }

        public override IEnumerable<IChildGrouping> Children => [];
    }
}