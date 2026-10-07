using System.Text.Json.Serialization;
using JEO3.Generation.Models;

namespace JEO3.Site.Dashboard
{
    public sealed class WSGridState
    {
        [JsonIgnore]
        public WSGridCoreState Core { get; set; } = new();
        [JsonIgnore]
        public WSGridSelectedState Selected { get; set; } = new();
        [JsonIgnore]
        public List<GraphRelationConstraint> ActiveConstraints { get; set; } = [];

        // Filtering
        public string SearchColumnName { get; set; } = string.Empty;
        public bool AutoFilterAllObjects { get; set; } = true;
        public bool AutoFilterRelations { get; set; } = true;
    }
}
