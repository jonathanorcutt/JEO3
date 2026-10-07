namespace JEO3.Schema
{
    public sealed class Index : ObjectBase, IIndex
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Index;
        public long? IndexId { get; internal init; }
        public IndexType IndexType { get; internal init; }
        public string? ProviderIndexType { get; internal init; }
        public bool IsUnique { get; internal init; }
        public bool IsPrimaryKey { get; internal init; }
        public bool IsUniqueConstraint { get; internal init; }
        public bool IsDisabled { get; internal init; }
        public bool IsFiltered { get; internal init; }
        public string? FilterDefinition { get; internal init; }
        public bool IsClustered { get; internal init; }
        public bool IsCovering { get; internal init; }
        public double? FragmentationPercentage { get; internal init; }
        public long? PageCount { get; internal init; }

        // ============================================================
        // NAVIGATION
        // ============================================================

        public ITable? Table { get; internal set; }

        public IReadOnlyList<IIndexColumn> Columns
        {
            get;
            internal set;
        } = Array.Empty<IIndexColumn>();

        public IReadOnlyList<IColumn> KeyColumns
        {
            get;
            internal set;
        } = Array.Empty<IColumn>();

        public IReadOnlyList<IColumn> IncludedColumns
        {
            get;
            internal set;
        } = Array.Empty<IColumn>();


        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => //Array.Empty<IChildGrouping>();
        new IChildGrouping[]
        {
            new ChildGrouping<IColumn> { Children = KeyColumns },
            new ChildGrouping<IColumn> { Children = IncludedColumns }
    };
}
}