using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class Column : ObjectBase, IColumn
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Column;
        [JsonInclude]
        public string TablePath { get; internal init; } = string.Empty;
        [JsonInclude]
        public long? ColumnId { get; internal init; }
        [JsonInclude]
        public string? DataType { get; internal set; }
        [JsonInclude]
        public string? ProviderDataType { get; internal init; }
        [JsonInclude]
        public int? OrdinalPosition { get; internal init; }
        [JsonInclude]
        public int? Precision { get; internal init; }
        [JsonInclude]
        public int? Scale { get; internal init; }
        [JsonInclude]
        public bool IsNullable { get; internal init; }
        [JsonInclude]
        public bool IsIdentity { get; internal init; }
        [JsonInclude]
        public bool IsComputed { get; internal init; }
        [JsonInclude]
        public bool IsGenerated { get; internal init; }
        [JsonInclude]
        public bool IsPrimaryKey { get; internal init; }
        [JsonInclude]
        public bool IsUnique { get; internal init; }
        [JsonInclude]
        public bool IsForeignKey { get; internal init; }
        [JsonInclude]
        public string? DefaultValue { get; internal init; }
        [JsonInclude]
        public string? ComputedExpression { get; internal init; }
        [JsonInclude]
        public string? CollationName { get; internal init; }
        [JsonInclude]
        public string? CharacterSetName { get; internal init; }
        [JsonInclude]
        public string? TableName { get; internal init; }


        [JsonInclude]
        public string Path => $"{SchemaName}.{TableName}.{Name}";
        [JsonInclude]
        public int? MaximumLength { get; internal init; }
        [JsonInclude]
        public string? ColumnDefault { get; internal init; } = string.Empty;
        [JsonInclude]
        public decimal? IdentitySeed { get; internal init; }
        [JsonInclude]
        public decimal? IdentityIncrement { get; internal init; }
        [JsonInclude]
        public decimal? IdentityLastValue { get; internal init; }
        [JsonInclude]
        public byte SystemTypeId { get; internal init; }
        [JsonInclude]
        public int UserTypeId { get; internal init; }
        [JsonInclude]
        public string ComputedDefinition { get; internal init; } = string.Empty;
        [JsonInclude]
        public int GeneratedAlwaysType { get; internal init; }
        [JsonInclude]
        public string GeneratedAlwaysTypeDescription { get; internal init; } = string.Empty;
        [JsonInclude]
        public int? EncryptionType { get; internal init; }
        [JsonInclude]
        public string? EncryptionTypeDescription { get; internal init; }
        [JsonInclude]
        public int XmlCollectionId { get; internal init; }
        [JsonInclude]
        public string Description { get; internal init; } = string.Empty;
        [JsonInclude]
        public string? UserTypeName { get; internal init; } = string.Empty;
        [JsonInclude]
        public string? SystemDataType { get; internal init; } = string.Empty;
        [JsonInclude]
        public bool IsMasked { get; internal init; }
        [JsonInclude]
        public bool IsTableType { get; internal init; }
        [JsonInclude]
        public bool IsFileStream { get; internal init; }
        [JsonInclude]
        public bool IsIdentityNotForReplication { get; internal init; }

        [JsonInclude]
        public bool IsPersisted { get; internal init; }
        [JsonInclude]
        public bool IsHidden { get; internal init; }
        [JsonInclude]
        public bool IsSparse { get; internal init; }
        [JsonInclude]
        public bool IsColumnSet { get; internal init; }
        [JsonInclude]
        public bool IsRowGuid { get; internal init; }
        // ============================================================
        // NAVIGATION
        // ============================================================

        public ITable? Table { get; internal set; } // SCOPE CHANGED INIT
        public IUserDefinedType UDT { get; internal set; }
        public IReadOnlyList<IColumnIndexLink> IndexLinks
        {
            get;
            internal set;
        } = Array.Empty<IColumnIndexLink>();

        public IReadOnlyList<IIndex> Indexes
        {
            get;
            internal set;
        } = Array.Empty<IIndex>();

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

        // ============================================================
        // DERIVED
        // ============================================================

        [JsonInclude]
        public bool IsIndexedInDatabase { get; internal init; }
        public bool IsIndexed =>
            IsIndexedInDatabase ||
            IndexLinks.Any(
                link =>
                    !link.IsIndexDisabled);

        public bool IsIndexKey =>
            IndexLinks.Any(
                link =>
                    !link.IsIndexDisabled &&
                    !link.IsIncludedColumn);

        public string SqlSafeName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name))
                    return string.Empty;

                string cleanName =
                    Name.Trim('[', ']')
                        .Replace("]", "]]");

                return $"[{cleanName}]";
            }
        }
        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => Array.Empty<IChildGrouping>(); 
        //new IChildGrouping[]
        //{
        //    new ChildGrouping<Index> { Children = Indexes }
        //};
    }
}