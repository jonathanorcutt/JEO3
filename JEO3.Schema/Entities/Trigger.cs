using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class Trigger : ObjectBase, ITrigger
    {
        [JsonInclude]
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Trigger;
        [JsonInclude]
        public long? ParentObjectId { get; internal init; }
        [JsonInclude]
        public bool IsDisabled { get; internal init; }
        [JsonInclude]
        public bool IsInsteadOfTrigger { get; internal init; }
        [JsonInclude]
        public string Definition { get; internal init; } = string.Empty;
        [JsonInclude]
        public byte[]? DefinitionHash { get; internal init; }


        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}
