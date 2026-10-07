namespace JEO3.Schema
{
    public sealed class Schema : ObjectBase, ISchema
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Schema;
        public DateTime? CreateDate { get; internal init; }
        public string? CollationName { get; internal init; }
        public short CompatibilityLevel { get; internal init; }
        public bool? IsReadSnapshotCommitted { get; internal init; }

        public IDatabase? Database { get; internal set; }
        public IReadOnlyList<ITable> Tables
        {
            get;
            internal set;
        } = Array.Empty<ITable>();
        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => new IChildGrouping[]
        {
            new ChildGrouping<ITable> { Name = "Tables", Children = Tables }
        };
    }
}
