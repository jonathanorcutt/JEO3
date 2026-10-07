using JEO3.Diagrams;

namespace JEO3.Site.Dashboard
{

    public sealed class WSDiagramState
    {
        public DiagramType DiagramType { get; set; } = DiagramType.Node;
        public double DiagramZoom { get; set; } = 0.2;
        public GraphLayoutShape LayoutShape
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    // Auto switch property buckets on runtime allocation change
                    ActiveParameters = (DiagramType == DiagramType.Node)
                        ? LayoutPresets.GetPreset(field, LayoutPresetType.JEO) : ERLayoutPresets.GetPreset(field, LayoutPresetType.JEO);
                }
            }
        } = GraphLayoutShape.KamadaKawai;
        public BaseLayoutParameters ActiveParameters { get; set; } = new KkParameters();
        public void ApplyPreset(LayoutPresetType presetType)
        {
            ActiveParameters = LayoutPresets.GetPreset(LayoutShape, presetType);
        }
    }
}