using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class Table : ObjectBase, ITable
    {
        #region Properties

        [JsonInclude]
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Table;
        [JsonInclude]
        public string TablePath => $"{SchemaName}.{Name}";
        [JsonInclude]
        public bool IsView { get; init; }
        [JsonInclude]
        public bool IsSystemObject { get; init; }
        [JsonInclude]
        public bool IsTemporary { get; init; }
        [JsonInclude]
        public bool IsMemoryOptimized { get; init; }
        [JsonInclude]
        public bool IsTemporal { get; init; }
        [JsonInclude]
        public long? Rows { get; init; }
        [JsonInclude]
        public string CreatedBy { get; init; } = string.Empty;
        [JsonInclude]
        public DateTime CreatedDate { get; init; }
        [JsonInclude]
        public DateTime DateLastModified { get; init; }
        [JsonInclude]
        public byte Durability { get; init; }
        [JsonInclude]
        public string DurabilityDescription { get; init; } = string.Empty;
        [JsonInclude]
        public string TableType { get; init; } = string.Empty;
        [JsonInclude]
        public int SchemaId { get; init; }
        [JsonInclude]
        public int TemporalType { get; init; }
        [JsonInclude]
        public string TemporalTypeDescription { get; init; } = string.Empty;
        [JsonInclude]
        public bool IsFileTable { get; init; }
        [JsonInclude]
        public int LobDataSpaceId { get; init; }
        [JsonInclude]
        public int HistoryTableObjectId { get; init; }
        [JsonInclude]
        public string LockEscalationDescription { get; init; } = string.Empty;
        [JsonInclude]
        public double TotalSpaceKB { get; init; }
        [JsonInclude]
        public double UsedSpaceKB { get; init; }
        [JsonInclude]
        public double UnusedSpaceKB { get; init; }

        [JsonInclude]
        public bool IsLargeTable => Rows > 1_000_000;
        public string SqlSafeName
        {
            get
            {
                string cleanSchema = !string.IsNullOrWhiteSpace(SchemaName)
                    ? $"[{SchemaName.Trim('[', ']')}]"
                    : "[dbo]";

                string cleanTable = $"[{Name.Trim('[', ']')}]";

                return $"{cleanSchema}.{cleanTable}";
            }
        }

        #endregion

        #region Navigation Properties

        public IDatabase? Database { get; internal set; }
        public ISchema? Schema { get; internal set; }
        public IReadOnlyList<IColumn> Columns
        {
            get;
            internal set;
        } = Array.Empty<IColumn>();
        public IReadOnlyList<IIndex> Indexes
        {
            get;
            internal set;
        } = Array.Empty<IIndex>();
        public IReadOnlyList<IColumnIndexLink> ColumnIndexLinks
        {
            get;
            internal set;
        } = Array.Empty<IColumnIndexLink>();
        public IReadOnlyList<IRelation> Relations
        {
            get;
            internal set;
        } = Array.Empty<IRelation>();
        public IReadOnlyList<IForeignKey> ForeignKeys
        {
            get;
            internal set;
        } = Array.Empty<IForeignKey>();
        public IReadOnlyList<IForeignKey> ReferencedByForeignKeys
        {
            get;
            internal set;
        } = Array.Empty<IForeignKey>();
        public IReadOnlyList<ISynonym> Synonymns
        {
            get;
            internal set;
        } = Array.Empty<ISynonym>();

        private IReadOnlyList<IRelation>? _childRelations;
        public IReadOnlyList<IRelation> ChildRelations => _childRelations ??= Relations.Where(v => v.ParentObjectId == ObjectId).ToList();
        private IReadOnlyList<IRelation>? _parentRelations;
        public IReadOnlyList<IRelation> ParentRelations => _parentRelations ??= Relations.Where(v => v.ParentObjectId != ObjectId).ToList();

        public IReadOnlyList<ICheckConstraint> CheckConstraints
        {
            get;
            internal set;
        } = Array.Empty<ICheckConstraint>();
        public IReadOnlyList<IMissingIndex> MissingIndexes
        {
            get;
            internal set;
        } = Array.Empty<IMissingIndex>();
        public IReadOnlyList<IStatistic> Statistics
        {
            get;
            internal set;
        } = Array.Empty<IStatistic>();
        public IReadOnlyList<ITrigger> Triggers
        {
            get;
            internal set;
        } = Array.Empty<ITrigger>();
        public IReadOnlyList<IExtendedProperty> ExtendedProperties
        {
            get;
            internal set;
        } = Array.Empty<IExtendedProperty>();
        public IReadOnlyList<ISynonym> Synonyms
        {
            get;
            internal set;
        } = Array.Empty<ISynonym>();

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => new IChildGrouping[]
        {
            new ChildGrouping<IColumn> { Name = "Columns", Children = Columns },
            new ChildGrouping<IIndex> { Name = "Indexes", Children = Indexes },
            new ChildGrouping<IMissingIndex> {  Name = "MissingIndexes",Children = MissingIndexes },
            //new ChildGrouping<IStatistic> { Name = "Statistics", Children = Statistics },
            new ChildGrouping<ITrigger> { Name = "Triggers", Children = Triggers },
            //new ChildGrouping<IExtendedProperty> {  Name = "ExtendedProperties",Children = ExtendedProperties }
        };

        #endregion
    }
}