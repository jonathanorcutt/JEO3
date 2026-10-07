namespace JEO3.Site.Dashboard
{
    public class DashboardUiTabItem
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public Dictionary<string, Type> Columns { get; set; } = [];
        public List<Dictionary<string, object>> Rows { get; set; } = [];
    }
}
