using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatUserDefinedTypeColumn
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
        public DateTime? CreateDate { get; private set; }
        [JsonInclude]
        public DateTime? ModifyDate { get; private set; }
        [JsonInclude]
        public int OrdinalPosition { get; private set; }
        [JsonInclude]
        public string? DataType { get; private set; }
        [JsonInclude]
        public string? ProviderDataType { get; private set; }
        [JsonInclude]
        public int? MaxLength { get; private set; }
        [JsonInclude]
        public int? Precision { get; private set; }
        [JsonInclude]
        public int? Scale { get; private set; }
        [JsonInclude]
        public bool IsOutput { get; private set; }
        [JsonInclude]
        public bool IsNullable { get; private set; }
        [JsonInclude]
        public string? DefaultValue { get; private set; }
    }
}
