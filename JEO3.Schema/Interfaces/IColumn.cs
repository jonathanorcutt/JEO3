namespace JEO3.Schema
{
    public interface IColumn : IObject
    {
        // JEO
        string TablePath { get; }
        public bool IsIndexKey { get; }
        long? ColumnId { get; }
        string? DataType { get; }
        string? ProviderDataType { get; }
        int? OrdinalPosition { get; }
        int? Precision { get; }
        int? Scale { get; }
        bool IsNullable { get; }
        bool IsIdentity { get; }
        bool IsComputed { get; }
        bool IsGenerated { get; }
        bool IsPrimaryKey { get; }
        bool IsUnique { get; }
        bool IsForeignKey { get; }
        bool IsIndexed { get; }
        string? DefaultValue { get; }
        string? ComputedExpression { get; }
        string? CollationName { get; }
        string? CharacterSetName { get; }
        string? TableName { get; }
        string Path { get; }
        int? MaximumLength { get; }
        string? ColumnDefault { get; }
        decimal? IdentitySeed { get; }
        decimal? IdentityIncrement { get; }
        decimal? IdentityLastValue { get; }
        byte SystemTypeId { get; }
        int UserTypeId { get; }
        string ComputedDefinition { get; }
        int GeneratedAlwaysType { get; }
        string GeneratedAlwaysTypeDescription { get; }
        int? EncryptionType { get; }
        string? EncryptionTypeDescription { get; }
        int XmlCollectionId { get; }
        string Description { get; }
        string? UserTypeName { get; }
        string? SystemDataType { get; }
        bool IsMasked { get; }
        bool IsTableType { get; }
        bool IsFileStream { get; }
        bool IsIdentityNotForReplication { get; }
        bool IsPersisted { get; }
        bool IsHidden { get; }
        bool IsSparse { get; }
        bool IsColumnSet { get; }
        bool IsRowGuid { get; }

        ITable? Table { get; }
        IReadOnlyList<IColumnIndexLink> IndexLinks { get; }
        IReadOnlyList<IIndex> Indexes { get; }
        IReadOnlyList<IForeignKey> ForeignKeys { get; }
        IReadOnlyList<IForeignKey> ReferencedByForeignKeys { get; }
        IReadOnlyList<ICheckConstraint> CheckConstraints { get; }
        IReadOnlyList<IMissingIndex> MissingIndexes { get; }
        IUserDefinedType UDT { get; }

        bool IsIndexedInDatabase { get; }
    }
}
