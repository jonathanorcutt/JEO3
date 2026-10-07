namespace JEO3.Diagrams
{
    public sealed class IsomParameters : BaseLayoutParameters
    {
        private const int _minEpochs = 1;
        private const int _maxEpochsLimit = 2000;
        private const int _minRadius = 1;
        private const int _maxRadiusLimit = 100;
        private const double _minCooling = 0.01;
        private const double _maxCooling = 0.99;

        [DiagramParameter("Max Epochs", _minEpochs, _maxEpochsLimit, 5)]
        public int MaxEpochs
        {
            get;
            set => field = Math.Clamp(value, _minEpochs, _maxEpochsLimit);
        } = 1500;

        [DiagramParameter("Initial Radius", _minRadius, _maxRadiusLimit, 1)]
        public int InitialRadius
        {
            get;
            set => field = Math.Clamp(value, _minRadius, _maxRadiusLimit);
        } = 5;

        [DiagramParameter("Min Radius", _minRadius, _maxRadiusLimit, 1)]
        public int MinRadius
        {
            get;
            set => field = Math.Clamp(value, _minRadius, InitialRadius); // Cannot exceed initial radius
        } = 1;

        [DiagramParameter("Cooling Factor", _minCooling, _maxCooling, _minCooling)]
        public double CoolingFactor
        {
            get;
            set => field = Math.Clamp(value, _minCooling, _maxCooling);
        } = 0.95;

        [DiagramParameter("Initial Adaptation", _minCooling, 5.0, 0.1)]
        public double InitialAdaptation { get; set; } = 0.5;

        [DiagramParameter("Min Adaptation", _minCooling, _maxCooling, _minCooling)]
        public double MinAdaptation { get; set; } = _minCooling;

        [DiagramParameter("Radius Constant Time", _minEpochs, _maxEpochsLimit, 100)]
        public int RadiusConstantTime { get; set; } = 1000;

        public override BaseLayoutParameters Clone()
        {
            return new IsomParameters
            {
                MaxEpochs = this.MaxEpochs,
                InitialRadius = this.InitialRadius,
                MinRadius = this.MinRadius,
                CoolingFactor = this.CoolingFactor,
                InitialAdaptation = this.InitialAdaptation,
                MinAdaptation = this.MinAdaptation,
                RadiusConstantTime = this.RadiusConstantTime
            };
        }
    }
}
