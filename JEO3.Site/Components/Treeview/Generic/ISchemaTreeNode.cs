using JEO3.Schema;

namespace JEO3.Site.Components.Treeview.Commented
{
    public interface ISchemaTreeNode
    {
        string Name { get; }
        string? Icon { get; }
        bool HasChildren { get; }
        Task<IEnumerable<ISchemaTreeNode>> GetChildrenAsync();
    }

    public class TableNode : ISchemaTreeNode
    {
        private readonly ITable _table;
        public TableNode(ITable table) => _table = table;

        public string Name => _table.Name;
        public string? Icon => "🗂";
        public bool HasChildren => _table.Columns.Count > 0;

        public Task<IEnumerable<ISchemaTreeNode>> GetChildrenAsync() =>
            Task.FromResult(_table.Columns.Select(c => (ISchemaTreeNode)new ColumnNode(c)));
    }

    public class ColumnNode : ISchemaTreeNode
    {
        private readonly IColumn _column;
        public ColumnNode(IColumn column) => _column = column;

        public string Name => _column.Name;
        public string? Icon => "▫";
        public bool HasChildren => false;

        public Task<IEnumerable<ISchemaTreeNode>> GetChildrenAsync() => Task.FromResult(Enumerable.Empty<ISchemaTreeNode>());
    }
}
