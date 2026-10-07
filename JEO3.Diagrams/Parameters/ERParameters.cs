namespace JEO3.Diagrams
{
    public sealed class ERParameters : BaseLayoutParameters
    {
        // Directional primitives
        private const int _minDir = 0;
        private const int _maxDir = 1;

        [DiagramParameter("Force Horizontal", _minDir, _maxDir, 1)]
        public bool ForceHorizontal
        {
            get;
            set => field = Math.Clamp(value ? 1 : 0, _minDir, _maxDir) == 1;
        } = false;

        [DiagramParameter("Force Vertical", _minDir, _maxDir, 1)]
        public bool ForceVertical
        {
            get;
            set => field = Math.Clamp(value ? 1 : 0, _minDir, _maxDir) == 1;
        } = false;

        // ER spacing primitives
        private const double _minSpacing = 10.0;
        private const double _maxSpacing = 800.0;
        private const double _stepSpacing = 5.0;

        [DiagramParameter("Parent/Child Spacing", _minSpacing, _maxSpacing, _stepSpacing)]
        public double ParentChildSpacing
        {
            get;
            set => field = Math.Clamp(value, _minSpacing, _maxSpacing);
        } = 60.0;

        [DiagramParameter("Sibling Spacing", _minSpacing, _maxSpacing, _stepSpacing)]
        public double SiblingSpacing
        {
            get;
            set => field = Math.Clamp(value, _minSpacing, _maxSpacing);
        } = 60.0;

        // ER alignment primitives
        private const int _minAlign = 0;
        private const int _maxAlign = 2;

        // 0 = Left, 1 = Center, 2 = Right
        [DiagramParameter("Alignment", _minAlign, _maxAlign, 1)]
        public int Alignment
        {
            get;
            set => field = Math.Clamp(value, _minAlign, _maxAlign);
        } = 1;

        //Clone
        [DiagramParameter("idealDistance", 50, 800, 1)]
        public int IdealDistance { get; set; } = 300;

        [DiagramParameter("temperature", 50, 800, 1)]
        public double Temperature { get; set; } = 350;

        [DiagramParameter("iterations", 100, 1000, 1)]
        public int Iterations { get; set; } = 400;

        public override BaseLayoutParameters Clone()
        {
            return new ERParameters
            {
                ForceHorizontal = this.ForceHorizontal,
                ForceVertical = this.ForceVertical,
                ParentChildSpacing = this.ParentChildSpacing,
                SiblingSpacing = this.SiblingSpacing,
                Alignment = this.Alignment,
                IdealDistance = this.IdealDistance,
                Temperature = this.Temperature,
                Iterations = this.Iterations
            };
        }
    }
}
