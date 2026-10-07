namespace JEO3.Schema
{
    public sealed class Database : ObjectBase, IDatabase
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Database;
        public DateTime? CreateDate { get; internal init; }
        public string? CollationName { get; internal init; }
        public short CompatibilityLevel { get; internal init; }
        public bool? IsReadSnapshotCommitted { get; internal init; }

        private IReadOnlyList<ITable>? _tables;
        public IReadOnlyList<ITable> Tables => _tables ??= Schemas.SelectMany(v => v.Tables).OrderBy(v => v.SchemaName).ThenBy(v => v.Name).ToList();
        private IReadOnlyList<ISchema> _schemas = Array.Empty<ISchema>();
        public IReadOnlyList<ISchema> Schemas
        {
            get => _schemas;
            internal set { _schemas = value; _tables = null; }
        }

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => new IChildGrouping[]
        {
            new ChildGrouping<ITable> { Name = "Schemas", Children = Schemas }
        };
    }
}
