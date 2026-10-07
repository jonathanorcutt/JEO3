using System.Text.Json.Serialization;

namespace JEO3.Engine.Models
{
    public sealed class FlatStoredProcedureDefinition
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
        public bool IsAutoExecuted { get; private set; }
        [JsonInclude]
        public bool IsExecutionReplicated { get; private set; }
        [JsonInclude]
        // You can join with sys.sql_modules to get the actual definition/text
        public string Definition { get; private set; }
        [JsonInclude]
        public byte[]? DefinitionHash { get; private set; }
    }
}
