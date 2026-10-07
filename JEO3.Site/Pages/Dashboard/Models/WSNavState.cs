namespace JEO3.Site.Pages.Dashboard.Models
{
    public sealed class WSNavState
    {
        public int SelectedTabIndex { get; set; } = 0;                          // flat index into the 12 RadzenTabsItems, unchanged
        public HashSet<int> ExpandedGroupIndices { get; set; } = new() { 0 };   // which accordion groups are open
    }
}
