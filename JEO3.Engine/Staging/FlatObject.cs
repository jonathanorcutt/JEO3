using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public abstract class FlatObject
    {
        [JsonInclude]
        public int ObjectId { get; set; }
        [JsonInclude]
        public string Name { get; set; }
        [JsonInclude]
        public string FullName { get; set; }
        [JsonInclude]
        public int SchemaId { get; set; }
        [JsonInclude]
        public DateTime CreateDate { get; set; }
        [JsonInclude]
        public DateTime ModifyDate { get; set; }
    }
}
