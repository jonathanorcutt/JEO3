namespace JEO3.Diagrams
{
    public sealed class SugiyamaParameters : BaseLayoutParameters
    {
        private const double _minGap = 10.0;
        private const double _maxGap = 500.0;

        [DiagramParameter("Direction", 0, 3, 1)]
        public DiagramLayoutDirection Direction { get; set; } = DiagramLayoutDirection.TopToBottom; // Map to GraphShape.LayoutDirection enum later

        [DiagramParameter("Layer Gap", _minGap, _maxGap, 1)]
        public double LayerGap
        {
            get;
            set => field = Math.Clamp(value, _minGap, _maxGap);
        } = 30.0;

        [DiagramParameter("Slice Gap", _minGap, _maxGap, 1)]
        public double SliceGap
        {
            get;
            set => field = Math.Clamp(value, _minGap, _maxGap);
        } = 30.0;

        [DiagramParameter("Optimize Width", 0, 1, 1)]
        public bool OptimizeWidth { get; set; } = true;

        [DiagramParameter("Edge Routing", 0, 1, 1)]
        public DiagramEdgeRoutingFunction EdgeRouting { get; set; } = DiagramEdgeRoutingFunction.Traditional;

        [DiagramParameter("Minimize Edge Length", 0, 1, 1)]
        public bool MinimizeEdgeLength { get; set; } = true;

        [DiagramParameter("Width Per Height", 0.1, 5.0, 0.1)]
        public double WidthPerHeight { get; set; } = 1.0;

        [DiagramParameter("Position Mode", -1, 3, 1)]
        public int PositionMode { get; set; } = -1;

        public override BaseLayoutParameters Clone()
        {
            return new SugiyamaParameters
            {
                Direction = this.Direction,
                LayerGap = this.LayerGap,
                SliceGap = this.SliceGap,
                OptimizeWidth = this.OptimizeWidth,
                EdgeRouting = this.EdgeRouting,
                MinimizeEdgeLength = this.MinimizeEdgeLength,
                WidthPerHeight = this.WidthPerHeight,
                PositionMode = this.PositionMode
            };
        }
    }
}
