using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Dashboard
{
    public partial class About
    {
        [Inject] private WSWorkspace W { get; set; } = new();
        [Parameter] public string Text { get; set; } = string.Empty;
        [Parameter] public string DatabaseName { get; set; } = string.Empty;
        [Parameter] public bool ShowDatabase { get; set; }
    }
}
