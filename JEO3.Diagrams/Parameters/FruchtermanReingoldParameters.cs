namespace JEO3.Diagrams
{
    public sealed class FruchtermanReingoldParameters : BaseLayoutParameters
    {
        private const double _minStep = 0.1;
        private const int _minIter = 10;
        private const int _maxIterLimit = 200;
        private const double _minMult = 0.1;
        private const double _maxMult = 25.0;

        [DiagramParameter("Max Iterations", _minIter, _maxIterLimit, _minIter)]
        public int MaxIterations
        {
            get;
            set => field = Math.Clamp(value, _minIter, _maxIterLimit);
        } = 200;

        [DiagramParameter("Attraction Multiplier", _minMult, _maxMult, _minStep)]
        public double AttractionMultiplier
        {
            get;
            set => field = Math.Clamp(value, _minMult, _maxMult);
        } = 0.7;

        [DiagramParameter("Repulsive Multiplier", _minMult, _maxMult, _minStep)]
        public double RepulsiveMultiplier
        {
            get;
            set => field = Math.Clamp(value, _minMult, _maxMult);
        } = 5.0;

        [DiagramParameter("Cooling Function", 0, 1, 1)]
        public DiagramCoolingFunction CoolingFunction { get; set; } = DiagramCoolingFunction.Exponential;
        [DiagramParameter("Ideal Edge Length", 0.0, _maxMult, 1.0)]
        public double IdealEdgeLength { get; set; } = 10.0;
        public double Lambda { get; set; } = 0.95;
        public override BaseLayoutParameters Clone()
        {
            return new FruchtermanReingoldParameters
            {
                MaxIterations = this.MaxIterations,
                AttractionMultiplier = this.AttractionMultiplier,
                RepulsiveMultiplier = this.RepulsiveMultiplier,
                CoolingFunction = this.CoolingFunction,
                IdealEdgeLength = this.IdealEdgeLength,
                Lambda = this.Lambda
            };
        }
    }
}
