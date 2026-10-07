using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.TileLayout
{
    public class GenericTileItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        // Grid Positions
        public int Col { get; set; } = 1;
        public int Row { get; set; } = 1;
        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;

        // Prevent JSON serialization errors by ignoring the UI logic property
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        [JsonIgnore]
        public RenderFragment? HeaderTemplate { get; set; }
    }
}
