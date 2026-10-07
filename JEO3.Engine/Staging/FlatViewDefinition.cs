using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatViewDefinition
    {
        [JsonInclude]
        public int ObjectId { get; private set; }
        [JsonInclude]
        public string Name { get; private set; }
        [JsonInclude]
        public string FullName { get; private set; }
        [JsonInclude]
        public int SchemaId { get; private set; }
        [JsonInclude]
        public DateTime CreateDate { get; private set; }
        [JsonInclude]
        public DateTime ModifyDate { get; private set; }
        [JsonInclude]
        public string DatabaseName { get; private set; }
        [JsonInclude]
        public string SchemaName { get; private set; }
        [JsonInclude]
        public bool IsReplicated { get; private set; }
        [JsonInclude]
        public bool WithCheckOption { get; private set; }
        [JsonInclude]
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; set; }
    }
}
