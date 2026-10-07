using JEO3.Diagrams;
using JEO3.Schema;
using Microsoft.AspNetCore.Components;

namespace JEO3.Site.Components.Database
{
    // Inherits existing TableNodeBase<TItem> — same NodeData/OnNodeSelected
    // contract Jarvis's traversal nodes already use. This is a second *view* of
    // the same node model, not a parallel node system.
    public partial class JErTableNode //: TableNodeBase<ITable>
    {
        // --- Assumption flags — rename these to match real ITable/IColumn shape ---
        // Column.IsPrimaryKey   -> swap for whatever PK indicator is (e.g. c.IsIdentity, c.KeyOrdinal > 0)
        // Column.DataType       -> swap for c.SqlDataType / c.TypeName / whatever you expose
        // ITable.MissingIndexes -> already confirmed present from Tables_DataTable join
        // ITable.Triggers       -> already confirmed present
        // ITable.CheckConstraints -> already confirmed present

        [Parameter]
        public TableNodeModel Node { get; set; }

        protected ITable NodeData => Node?.Table;

        // Cache the sorted column list so we don't allocate a new sorted enumerable on every render.
        // Rebuild only when the Node parameter changes.
        private TableNodeModel? _lastNode;
        private IReadOnlyList<IColumn> _orderedColumnsCache = Array.Empty<IColumn>();

        protected IReadOnlyList<IColumn> OrderedColumns => _orderedColumnsCache;

        protected override void OnParametersSet()
        {
            if (!ReferenceEquals(Node, _lastNode))
            {
                _lastNode = Node;
                _orderedColumnsCache = NodeData?.Columns?
                    .OrderByDescending(c => c.IsPrimaryKey)
                    .ToList() ?? [];
            }
        }

        [Parameter]
        public bool ShowDiagnosticBadges { get; set; } = true;

        [Parameter]
        public int MaxVisibleColumns { get; set; } = 12; // beyond this, internal scroll instead of growing the card forever

        protected int MissingIndexCount => NodeData?.MissingIndexes?.Count ?? 0;
        protected int TriggerCount => NodeData?.Triggers?.Count ?? 0;
        protected int CheckConstraintCount => NodeData?.CheckConstraints?.Count ?? 0;
        protected int IndexCount => NodeData?.Indexes?.Count ?? 0;
        protected int FkCount => NodeData?.ForeignKeys?.Count ?? 0;
        protected int RefFkCount => NodeData?.ReferencedByForeignKeys?.Count ?? 0;
        protected int StatisticsCount => NodeData?.Statistics?.Count ?? 0;
        protected int ExtendedPropertiesCount => NodeData?.ExtendedProperties?.Count ?? 0;

        protected bool HasAnyDiagnosticFlags =>
            ShowDiagnosticBadges && (MissingIndexCount > 0 || TriggerCount > 0);
    }
}
