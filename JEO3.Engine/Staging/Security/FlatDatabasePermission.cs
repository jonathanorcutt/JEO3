using System.Text.Json.Serialization;

namespace JEO3.Engine.Entities
{
    public sealed class FlatDatabasePermission
    {
        [JsonInclude]
        public int GranteePrincipalId { get; private set; }
        [JsonInclude]
        public string? GranteeName { get; private set; }
        [JsonInclude]
        public int GrantorPrincipalId { get; private set; }
        [JsonInclude]
        public string? GrantorName { get; private set; }
        [JsonInclude]
        public string? StateDesc { get; private set; } // GRANT, GRANT_WITH_GRANT_OPTION, DENY, REVOKE
        [JsonInclude]
        public string? PermissionName { get; private set; } // SELECT, INSERT, UPDATE, EXECUTE, etc.
        [JsonInclude]
        public int MajorId { get; private set; }
        [JsonInclude]
        public int MinorId { get; private set; }
        [JsonInclude]
        public string? ClassDesc { get; private set; }
        [JsonInclude]
        public string? SecurableSchemaName { get; private set; }
        [JsonInclude]
        public string? SecurableName { get; private set; }
    }
}
