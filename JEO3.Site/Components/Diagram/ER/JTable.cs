using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace JEO3.Site.Components.Database
{
    public sealed class JTable : NodeModel
    {
        public JTable(string id, Point position = null) : base(id, position)
        {
            Columns =
            [
                new JColumn
                {
                    Name = id,
                    Type = ColumnType.Integer,
                    Primary = true
                },
                new JColumn
                {
                    Name = "Test",
                    Type = ColumnType.Integer
                }
            ];

            AddPort(Columns[0], PortAlignment.Right);
            AddPort(Columns[1], PortAlignment.Left);
        }

        public string Name { get; set; } = "Table";
        public List<JColumn> Columns { get; }
        public bool HasPrimaryColumn => Columns.Any(c => c.Primary);

        public JColumnPort GetPort(JColumn column)
        {
            return Ports.Cast<JColumnPort>().FirstOrDefault(p => p.Column == column);
        }

        public void AddPort(JColumn column, PortAlignment alignment)
        {
            AddPort(new JColumnPort(this, column, alignment));
        }
    }
}
