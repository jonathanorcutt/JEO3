using System.Text.Json.Serialization;

namespace JEO3.Engine.Entities
{
    public sealed class FlatRoleMembership
    {
        [JsonInclude]
        public int RolePrincipalId { get; private set; }
        [JsonInclude]
        public string? RoleName { get; private set; }
        [JsonInclude]
        public int MemberPrincipalId { get; private set; }
        [JsonInclude]
        public string? MemberName { get; private set; }
    }
}
