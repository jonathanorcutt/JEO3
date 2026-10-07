namespace JEO3.Schema
{
    public interface ITable : IObject
    {
        // JEO        
        string TablePath { get; }

        bool IsView { get; }
        bool IsSystemObject { get; }
        bool IsTemporary { get; }
        bool IsMemoryOptimized { get; }
        bool IsTemporal { get; }
        long? Rows { get; }
        string CreatedBy { get; init; }
        DateTime CreatedDate { get; init; }
        DateTime DateLastModified { get; init; }
        byte Durability { get; init; }
        string DurabilityDescription { get; init; }
        string TableType { get; init; }
        int SchemaId { get; init; }
        int TemporalType { get; init; }
        string TemporalTypeDescription { get; init; }
        bool IsFileTable { get; init; }
        int LobDataSpaceId { get; init; }
        int HistoryTableObjectId { get; init; }
        string LockEscalationDescription { get; init; }
        double TotalSpaceKB { get; init; }
        double UsedSpaceKB { get; init; }
        double UnusedSpaceKB { get; init; }
        bool IsLargeTable => Rows > 1_000_000;
        string SqlSafeName { get; }
        IReadOnlyList<IColumn> Columns { get; }
        IReadOnlyList<IIndex> Indexes { get; }
        IReadOnlyList<IColumnIndexLink> ColumnIndexLinks { get; }
        IReadOnlyList<IRelation> Relations { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<IForeignKey> ReferencedByForeignKeys { get; }
        IReadOnlyList<IRelation> ParentRelations { get; }
        IReadOnlyList<IRelation> ChildRelations { get; }
        IReadOnlyList<ITrigger> Triggers { get; }
        IReadOnlyList<ICheckConstraint> CheckConstraints { get; }
        IReadOnlyList<IMissingIndex> MissingIndexes { get; }
        IReadOnlyList<ISynonym> Synonyms { get; }
        IReadOnlyList<IExtendedProperty> ExtendedProperties { get; }
        IReadOnlyList<IStatistic> Statistics { get; }
    }
}
