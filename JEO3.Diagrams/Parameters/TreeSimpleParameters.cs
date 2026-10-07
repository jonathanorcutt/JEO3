namespace JEO3.Diagrams
{
    public sealed class TreeSimpleParameters : BaseLayoutParameters
    {
        private const double _minGap = 10.0;
        private const double _maxGap = 300.0;

        [DiagramParameter("Direction", 0, 3, 1)]
        public DiagramLayoutDirection Direction { get; set; } = DiagramLayoutDirection.TopToBottom; // Map to GraphShape.LayoutDirection enum later

        [DiagramParameter("Layer Gap", _minGap, _maxGap, 1)]
        public double LayerGap
        {
            get;
            set => field = Math.Clamp(value, _minGap, _maxGap);
        } = 30.0;

        [DiagramParameter("Vertex Gap", _minGap, _maxGap, 1)]
        public double VertexGap
        {
            get;
            set => field = Math.Clamp(value, _minGap, _maxGap);
        } = 20.0;

        public SpanTreeGeneration SpanTreeGeneration { get; set; } = SpanTreeGeneration.BFS;

        public override BaseLayoutParameters Clone()
        {
            return new TreeSimpleParameters
            {
                Direction = this.Direction,
                LayerGap = this.LayerGap,
                VertexGap = this.VertexGap,
                SpanTreeGeneration = this.SpanTreeGeneration
            };
        }
    }
}
