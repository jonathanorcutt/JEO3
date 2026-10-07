namespace JEO3.Site.Dashboard
{
    public sealed class WSAppearanceState
    {
        public string ActivePrimaryColor { get; set; } = "#4a6b4c";
        // Show / Hide
        public bool ShowMonitorGauge { get; set; } = true;
        public bool ShowAnimations { get; set; } = true;
        // Readiness
        public bool IsSchemaLoaded { get; set; } = false;
        public bool IsQueryBusy { get; set; } = false;
        public bool IsMonacoEditorReady { get; set; } = false;
        public bool IsShowingPageLoadAsciiArt { get; set; } = true;
        // Themes
        public SqlEditorTheme ERDiagramTheme { get; set; } = SqlEditorTheme.AS400;
        public SqlEditorTheme DiagramTheme { get; set; } = SqlEditorTheme.AS400;
        public SqlEditorTheme EditorTheme { get; set; } = SqlEditorTheme.AS400;
        // Internal Sidebars
        public bool IsSidebarExpandedDiagram { get; set; } = false;
        public bool IsSidebarExpandedERDiagram { get; set; } = false;
        public bool IsSidebarExpandedQueryEditor { get; set; } = true;
        // Page Sidebars
        public bool IsSidebarExpandedRight { get; set; } = true;
        public bool IsSidebarExpandedLeft { get; set; } = false;
        // Fullscreen
        public bool IsPaneFullScreenResults { get; set; } = false;
        public bool IsPaneFullScreenQueryEditor { get; set; } = false;
        // Pane Collapsed/Expanded
        public bool IsPaneCollapsedTables { get; set; } = false;
        public bool IsPaneCollapsedColumns { get; set; } = false;
        public bool IsPaneCollapsedAllObjects { get; set; } = false;
        public bool IsPaneCollapsedRelations { get; set; } = false;
        public bool IsPaneExpandedResults { get; set; } = false;
        public bool IsToolboxCollapsed { get; set; } = true;
    }
}
