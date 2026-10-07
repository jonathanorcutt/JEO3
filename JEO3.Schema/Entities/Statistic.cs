using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public sealed class Statistic : ObjectBase, IStatistic
    {
        public override DatabaseObjectType ObjectType => DatabaseObjectType.Statistic;
        [JsonInclude]
        public long? ParentObjectId { get; internal init; }
        [JsonInclude]
        public int StatsId { get; internal init; }
        [JsonInclude]
        public bool IsAutoCreated { get; internal init; }
        [JsonInclude]
        public bool IsUserCreated { get; internal init; }
        [JsonInclude]
        public bool HasFilter { get; internal init; }
        [JsonInclude]
        public string? FilterDefinition { get; internal init; }
        [JsonInclude]
        public DateTime? CreateDate { get; internal init; }
        [JsonInclude]
        public DateTime? ModifyDate { get; internal init; }
        [JsonInclude]
        public string? CreatedBy { get; internal init; }


        public override IObject? Parent { get; internal set; }
        public override IEnumerable<IChildGrouping> Children { get; } = Array.Empty<IChildGrouping>();
    }
}
