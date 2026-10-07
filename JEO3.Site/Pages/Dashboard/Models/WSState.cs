using JEO3.Site.Pages.Dashboard.Models;

namespace JEO3.Site.Dashboard
{
    /// <summary>
    /// Holds UI presentation state values to separate layout overhead from metadata engines.
    /// </summary>
    public sealed class WSUiState
    {
        // ER
        public WSDiagramState ERDiagram { get; set; } = new() { DiagramType = Diagrams.DiagramType.ERD, DiagramZoom = .2, LayoutShape = Diagrams.GraphLayoutShape.EntityRelation };
        public WSDiagramSettingsState ERSettings { get; set; } = new();

        // Node
        public WSDiagramState Diagram { get; set; } = new() { DiagramType = Diagrams.DiagramType.Node, DiagramZoom = 1.2, LayoutShape = Diagrams.GraphLayoutShape.KamadaKawai };
        public WSDiagramSettingsState Settings { get; set; } = new();

        public WSNavState Nav { get; set; } = new();
        public WSUiTabState Tabs { get; set; } = new();
        public WSAppearanceState Appearance { get; set; } = new();
        public WSGridState Grids { get; set; } = new();
    }
}
