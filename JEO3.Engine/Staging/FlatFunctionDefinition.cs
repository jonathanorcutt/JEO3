using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatFunctionDefinition
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
        public string DatabaseName { get; private set; } = string.Empty;
        [JsonInclude]
        public string SchemaName { get; private set; } = string.Empty;
        [JsonInclude]
        public bool IsReplicated { get; private set; }
        [JsonInclude]
        public bool WithCheckOption { get; private set; }
        [JsonInclude]
        public string Definition { get; private set; } = string.Empty;
        [JsonInclude]
        public byte[]? DefinitionHash { get; set; }
        [JsonInclude]
        public string FunctionType { get; private set; } = string.Empty;
        [JsonInclude]
        public string ObjectType { get; private set; } = string.Empty;
        [JsonInclude]
        public string ReturnType { get; private set; } = string.Empty;
        [JsonInclude]
        public string ProviderReturnType { get; private set; } = string.Empty;
        [JsonInclude]
        public string Identifier { get; private set; } = string.Empty;
        [JsonInclude]
        public bool IsSystemObject { get; private set; }
        [JsonInclude]
        public bool IsDeterministic { get; private set; }
    }
}
