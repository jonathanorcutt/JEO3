namespace JEO3.Diagrams
{
    public sealed class TreeBalloonParameters : BaseLayoutParameters
    {
        private const int _minRadius = 0;
        private const int _maxRadius = 500;
        private const float _minBorder = 1;
        private const float _maxBorder = 100;

        [DiagramParameter("Min Radius", _minRadius, _maxRadius, 5)]
        public int MinRadius
        {
            get;
            set => field = Math.Clamp(value, _minRadius, _maxRadius);
        } = 2;

        [DiagramParameter("Border", _minBorder, _maxBorder, 1)]
        public float Border
        {
            get;
            set => field = Math.Clamp(value, _minBorder, _maxBorder);
        } = 20.0F;

        public override BaseLayoutParameters Clone()
        {
            return new TreeBalloonParameters
            {
                MinRadius = this.MinRadius,
                Border = this.Border
            };
        }
    }
}
