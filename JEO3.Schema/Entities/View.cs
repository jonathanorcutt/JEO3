namespace JEO3.Schema
{
    public sealed class View : ObjectBase, IView
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.View;
        public string? Definition { get; internal init; }
        public bool IsSystemObject { get; internal init; }
        public bool IsEncrypted { get; internal init; }
        public bool IsMaterialized { get; internal init; }
        public IReadOnlyList<IColumn> Columns { get; internal set; } = Array.Empty<IColumn>();
        public IReadOnlyList<IRelation> Relations { get; internal set; } = Array.Empty<IRelation>();

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}