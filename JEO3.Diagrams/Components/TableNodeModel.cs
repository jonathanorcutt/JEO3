using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
//using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using JEO3.Schema;

namespace JEO3.Diagrams
{
    public class TableNodeModel : NodeModel
    {
        public string BackgroundColor { get; set; } = "#ffffff";
        public string TextColor { get; set; } = "#111215";
        public int Depth { get; set; } = 0;
        public int FontSize { get; set; } = 0;
        public int Padding { get; set; } = 5;
        public int MinWidth = 40;
        public string CssClassName => string.Format("jeo3-node-depth-{0}", Depth);

        public bool IsRootNode { get; set; } = false;
        public List<NodeModel> ChildNodes { get; set; } = [];

        // Snapshot anchor to measure real spatial distance delta
        private Point _lastKnownPosition;

        private const double HeaderHeight = 26;
        private const double RowHeight = 20;
        private const double NodeWidth = 110;

        public List<TableColumnNodeModel> ColumnNodes { get; } = [];

        public TableNodeModel(Point position, ITable table = null) : base(position)
        {
            _lastKnownPosition = position;
            this.Changed += OnNodeChanged;

            if (table?.Columns != null)
            {
                foreach (var column in table.Columns.OrderByDescending(c => c.IsPrimaryKey))
                {
                    // position is nominal — these aren't independently rendered/dragged,
                    // they exist for sizing + future column-anchor links, not standalone display
                    ColumnNodes.Add(new TableColumnNodeModel(column, position));
                }

                // This is the part that actually matters for FSA/KK overlap removal:
                // real height in, real overlap detection out.
                var computedHeight = HeaderHeight + (ColumnNodes.Count * RowHeight);
                this.Size = new Size(NodeWidth, computedHeight);
            }
        }

        // New method only — existing constructor and all its current callers untouched.
        public List<IColumn> ColumnInfos { get; private set; } = [];
        public ITable Table { get; set; }

        private const double ErHeaderHeight = 25;
        private const double ErRowHeight = 19;
        private const double ErNodeWidth = 150;

        public void PopulateColumnInfo(ITable table)
        {
            if (table?.Columns == null) return;
            Table = table;
            ColumnInfos = table.Columns
                .OrderByDescending(c => c.IsPrimaryKey)
                .Select(c => c)
                .ToList();

            this.Size = new Size(ErNodeWidth, ErHeaderHeight + (ColumnInfos.Count * ErRowHeight));
        }
        private void OnNodeChanged(Model model)
        {
            // Safety check: Ensure the node isn't calculating against an uninitialized map vector
            if (this.Position == null || _lastKnownPosition == null) return;

            // Calculate true delta movement offsets
            double deltaX = this.Position.X - _lastKnownPosition.X;
            double deltaY = this.Position.Y - _lastKnownPosition.Y;

            // If the node hasn't actually shifted space (e.g., selection change, color swap), abort
            if (Math.Abs(deltaX) < 0.01 && Math.Abs(deltaY) < 0.01) return;

            // Unsubscribe immediately to eliminate cyclic double-binding execution traps
            this.Changed -= OnNodeChanged;

            // Mirror the physical pointer translation vector across all structural layout descendants
            foreach (var child in ChildNodes)
            {
                if (child == null) continue;

                // Shift coordinates safely
                child.SetPosition(child.Position.X + deltaX, child.Position.Y + deltaY);
            }

            // Commit the new structural snapshot position coordinate
            _lastKnownPosition = this.Position;

            // Re-engage event listener loops
            this.Changed += OnNodeChanged;
        }
    }
}
