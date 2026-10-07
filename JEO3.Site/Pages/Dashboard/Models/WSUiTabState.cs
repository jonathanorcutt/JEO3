namespace JEO3.Site.Dashboard
{
    public sealed class WSUiTabState
    {
        // Query Result Tabs
        public int SelectedIndex { get; set; } = 0;
        public int NextTabId { get; set; } = 1;
        public List<DashboardUiTabItem> Tabs { get; set; } = [];
        public IDictionary<string, Type> ResultColumns { get; set; } = new Dictionary<string, Type>();
    }
}
