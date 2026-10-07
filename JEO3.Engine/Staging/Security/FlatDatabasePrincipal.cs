using System.Text.Json.Serialization;

namespace JEO3.Engine.Entities
{
    namespace JEO3.Engine.Models
    {
        public sealed class FlatDatabasePrincipal
        {
            [JsonInclude]
            public int PrincipalId { get; private set; }
            [JsonInclude]
            public string? Name { get; private set; }
            [JsonInclude]
            public string? PrincipalType { get; private set; } // 'S', 'U', 'R', 'G', etc.
            [JsonInclude]
            public string? TypeDescription { get; private set; } // SQL_USER, WINDOWS_USER, DATABASE_ROLE, etc.
            [JsonInclude]
            public int? DefaultSchemaId { get; private set; }
            [JsonInclude]
            public string? DefaultSchemaName { get; private set; }
            [JsonInclude]
            public bool IsFixedRole { get; private set; }
            [JsonInclude]
            public int? OwningPrincipalId { get; private set; }
        }
    }
}
