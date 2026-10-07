using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatUserDefinedTypeDefinition
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
        public string Identifier { get; private set; }
        [JsonInclude]
        /// Possible values:
        /// CLR_UDT, TABLE_TYPE, or ALIAS_UDT.
        /// </summary>
        public string FunctionType { get; private set; }
        [JsonInclude]
        public DateTime? CreateDate { get; private set; }
        [JsonInclude]
        public DateTime? ModifyDate { get; private set; }
        [JsonInclude]
        public string DatabaseName { get; private set; }
        [JsonInclude]
        public string SchemaName { get; private set; }
        [JsonInclude]
        public string ReturnType { get; private set; }
        [JsonInclude]
        public string ProviderReturnType { get; private set; }
        [JsonInclude]
        public bool IsSystemObject { get; private set; }
        [JsonInclude]
        public bool IsDeterministic { get; private set; }
        [JsonInclude]
        public bool IsReplicated { get; private set; }
        [JsonInclude]
        public bool WithCheckOption { get; private set; }
        [JsonInclude]
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }
        [JsonInclude]
        public int? MaxLength { get; private set; }
        [JsonInclude]
        public int? Precision { get; private set; }
        [JsonInclude]
        public int? Scale { get; private set; }
    }
}
