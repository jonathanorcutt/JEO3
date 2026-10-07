using System.Text.Json.Serialization;

namespace JEO3.Schema
{
    public abstract class ObjectBase : IObject
    {
        public abstract DatabaseObjectType ObjectType { get; }
        [JsonInclude]
        public int? ObjectId { get; internal init; }
        [JsonInclude]
        public string? Identifier { get; internal init; }
        [JsonInclude]
        public string? DatabaseName { get; internal init; }
        [JsonInclude]
        public string? SchemaName { get; internal init; }
        [JsonInclude]
        public string Name { get; internal init; } = string.Empty;
        [JsonInclude]
        public string? FullName { get; internal init; }

        /// <summary>
        /// The object this one is nested under in the schema hierarchy (e.g. a column's table), or null at the root.
        /// </summary>
        public abstract IObject? Parent { get; internal set; }

        /// <summary>
        /// Objects nested directly under this one (e.g. a table's columns), empty if none.
        /// </summary>
        public abstract IEnumerable<IChildGrouping> Children { get; }

    }
}