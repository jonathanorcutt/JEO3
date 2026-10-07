using Blazor.Diagrams.Core.Models;
using JEO3.Schema;

namespace JEO3.Diagrams
{
    // Lightweight — no ports, no CanAttachTo, no link validation.
    // Exists purely so the parent table's Size reflects real column count.
    public sealed class TableColumnNodeModel : NodeModel
    {
        public string ColumnName { get; }
        public string DataType { get; }
        public bool IsPrimaryKey { get; }
        public bool IsForeignKey { get; }

        public TableColumnNodeModel(IColumn column, Blazor.Diagrams.Core.Geometry.Point position) : base(position)
        {
            ColumnName = column.Name;
            DataType = column.DataType;       // rename to real property
            IsPrimaryKey = column.IsPrimaryKey;
            IsForeignKey = column.IsForeignKey;
        }
    }
}
