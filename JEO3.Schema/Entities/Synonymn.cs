using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class Synonym : ObjectBase, ISynonym
    {
        [JsonInclude]
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Synonym;
        [JsonInclude]
        public int Id { get; internal init; }
        [JsonInclude]
        public int SchemaId { get; internal init; }
        [JsonInclude]
        public string TargetSchemaName { get; internal init; } = string.Empty;
        [JsonInclude]
        public string TargetTableName { get; internal init; } = string.Empty;
        [JsonInclude]
        public string BaseObjectName { get; internal init; } = string.Empty;
        [JsonInclude]
        public int? PrincipalId { get; internal init; }
        [JsonInclude]
        public int ParentObjectId { get; internal init; }
        [JsonInclude]
        public string Type { get; internal init; } = string.Empty;
        [JsonInclude]
        public string TypeDesc { get; internal init; } = string.Empty;
        [JsonInclude]
        public DateTime CreateDate { get; internal init; }
        [JsonInclude]
        public DateTime ModifyDate { get; internal init; }
        [JsonInclude]
        public bool IsMsShipped { get; internal init; }
        [JsonInclude]
        public bool IsPublished { get; internal init; }
        [JsonInclude]
        public bool IsSchemaPublished { get; internal init; }
        [JsonInclude]
        public Table? TargetTable { get; internal set; }


        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}
