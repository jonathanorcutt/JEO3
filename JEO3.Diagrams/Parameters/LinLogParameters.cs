namespace JEO3.Diagrams
{
    public sealed class LinLogParameters : BaseLayoutParameters
    {
        private const int _minIter = 10;
        private const int _maxIterLimit = 1000;
        private const double _minExp = -5.0;
        private const double _maxExp = 5.0;
        private const double _minStepExponent = 0.1;
        private const double _minGrav = 0.0;
        private const double _maxGrav = 10.0;

        [DiagramParameter("Max Iterations", _minIter, _maxIterLimit, 10)]
        public int MaxIterations
        {
            get;
            set => field = Math.Clamp(value, _minIter, _maxIterLimit);
        } = 100;

        [DiagramParameter("Attractive Exponent", _minExp, _maxExp, _minStepExponent)]
        public double AttractionExponent
        {
            get;
            set => field = Math.Clamp(value, _minExp, _maxExp);
        } = 1.0;

        [DiagramParameter("Repulsive Exponent", _minExp, _maxExp, _minStepExponent)]
        public double RepulsiveExponent
        {
            get;
            set => field = Math.Clamp(value, _minExp, _maxExp);
        } = 0.0;

        [DiagramParameter("Gravitation Exponent", _minGrav, _maxGrav, _minStepExponent)]
        public double GravitationMultiplier
        {
            get;
            set => field = Math.Clamp(value, _minGrav, _maxGrav);
        } = 0.1;

        public override BaseLayoutParameters Clone()
        {
            return new LinLogParameters
            {
                MaxIterations = this.MaxIterations,
                AttractionExponent = this.AttractionExponent,
                RepulsiveExponent = this.RepulsiveExponent,
                GravitationMultiplier = this.GravitationMultiplier
            };
        }
    }
}
