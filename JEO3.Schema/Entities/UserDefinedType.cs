using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class UserDefinedType : ObjectBase, IUserDefinedType
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.UserDefinedType;
        [JsonInclude]
        public UserDefinedTypeKind TypeKind { get; internal init; }
        [JsonInclude]
        public string? BaseType { get; internal init; }
        [JsonInclude]
        public string? ProviderBaseType { get; internal init; }
        [JsonInclude]
        public int? MaxLength { get; internal init; }
        [JsonInclude]
        public int? Precision { get; internal init; }
        [JsonInclude]
        public int? Scale { get; internal init; }
        [JsonInclude]
        public bool IsNullable { get; internal init; }
        [JsonInclude]
        public string Definition { get; internal init; } = string.Empty;
        [JsonInclude]
        public byte[]? DefinitionHash { get; internal init; }
        [JsonInclude]
        public DateTime? CreateDate { get; internal init; }
        [JsonInclude]
        public DateTime? ModifyDate { get; internal init; }
        [JsonInclude]
        public int SchemaId { get; internal init; }
        [JsonInclude]
        public bool IsSystemObject { get; internal init; }
        [JsonInclude]
        public bool IsDeterministic { get; internal init; }
        [JsonInclude]
        public bool IsReplicated { get; internal init; }
        [JsonInclude]
        public bool WithCheckOption { get; internal init; }

        public IReadOnlyList<IUserDefinedTypeColumn> Columns { get; internal set; } = Array.Empty<IUserDefinedTypeColumn>();

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}