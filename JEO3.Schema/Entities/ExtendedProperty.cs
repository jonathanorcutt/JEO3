using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class ExtendedProperty : ObjectBase, IExtendedProperty
    {
        [JsonInclude]
        public override DatabaseObjectType ObjectType => DatabaseObjectType.ExtentedProperty;
        [JsonInclude]
        public int MajorId { get; internal init; }
        [JsonInclude]
        public int MinorId { get; internal init; }
        [JsonInclude]
        public int Class { get; internal init; }
        [JsonInclude]
        public string? ClassDescription { get; internal init; }
        [JsonInclude]
        public string? PropertyName { get; internal init; }
        [JsonInclude]
        public string? Value { get; internal init; }
        [JsonInclude]
        public string? ObjectTypeDescription { get; internal init; }
        [JsonInclude]
        public int? ParentObjectId { get; internal init; }
        [JsonInclude]
        public string? ParentObjectName { get; internal init; }
        [JsonInclude]
        public string? ColumnName { get; internal init; }
        [JsonInclude]
        public string? IndexName { get; internal init; }

        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children => Array.Empty<IChildGrouping>();
    }
}
