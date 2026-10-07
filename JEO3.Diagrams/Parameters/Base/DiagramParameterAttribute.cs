namespace JEO3.Diagrams
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DiagramParameterAttribute : Attribute
    {
        public string Label { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public double Step { get; set; } = 0.1;

        public DiagramParameterAttribute(string label, double min, double max, double step = 0.1)
        {
            Label = label;
            Min = min;
            Max = max;
            Step = step;
        }
    }
}
