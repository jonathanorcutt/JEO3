namespace JEO3.Diagrams
{
    public sealed class KkParameters : BaseLayoutParameters
    {
        private const double _minStep = 0.1;
        private const int _minIterations = 10;
        private const int _maxIterationsLimit = 1000;
        private const double _minDisconnected = 0.1;
        private const double _maxDisconnected = 25.0;
        private double _maxMultiplier = 25;

        private const double _minK = 0.1;
        private const double _maxK = 5.0;
        private const double _minLengthFactor = 0.1;
        private const double _maxLengthFactor = 5.0;

        [DiagramParameter("Max Iterations", _minIterations, _maxIterationsLimit, _minIterations)]
        public int MaxIterations
        {
            get;
            set => field = Math.Clamp(value, _minIterations, _maxIterationsLimit);
        } = 200;

        [DiagramParameter("Disconnected Multiplier", _minDisconnected, _maxDisconnected, _minStep)]
        public double DisconnectedMultiplier
        {
            get;
            set => field = Math.Clamp(value, _minDisconnected, _maxMultiplier);
        } = 0.5;

        [DiagramParameter("K", 1.0, 5.0, _minStep)]
        public double K
        {
            get;
            set => field = Math.Clamp(value, _minK, _maxK);
        } = 1.0;

        [DiagramParameter("Length Factor", _minLengthFactor, _maxLengthFactor, _minStep)]
        public double LengthFactor
        {
            get;
            set => field = Math.Clamp(value, _minLengthFactor, _maxLengthFactor);
        } = 0.5;

        [DiagramParameter("Exchange Vertices", 0, 1, 1)]
        public bool ExchangeVertices { get; set; } = false;

        public override BaseLayoutParameters Clone()
        {
            return new KkParameters
            {
                MaxIterations = this.MaxIterations,
                DisconnectedMultiplier = this.DisconnectedMultiplier,
                K = this.K,
                LengthFactor = this.LengthFactor,
                ExchangeVertices = this.ExchangeVertices
            };
        }
    }
}
