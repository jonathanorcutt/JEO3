namespace JEO3.Diagrams
{
    public sealed class RandomParameters : BaseLayoutParameters
    {
        [DiagramParameter("X Offset", 0, 800, 1)]
        public double XOffset { get; set; } = 0;
        [DiagramParameter("Y Offset", 0, 800, 1)]
        public double YOffset { get; set; } = 0;

        public override BaseLayoutParameters Clone()
        {
            return new RandomParameters
            {
                XOffset = this.XOffset,
                YOffset = this.YOffset
            };
        }
    }
}
