using JEO3.Schema;

namespace JEO3.Site.Dashboard
{
    public sealed class WSGridCoreState
    {
        public IEnumerable<ITable> Tables { get; set; } = Array.Empty<ITable>();
        public IEnumerable<IColumn> Columns { get; set; } = Array.Empty<IColumn>();
        public IReadOnlyList<IRelationColumnPair> Relations { get; set; } = Array.Empty<IRelationColumnPair>();
        public IEnumerable<IColumn> AllObjects { get; set; } = Array.Empty<Column>();
        public IEnumerable<IProcedure> StoredProcedureDefinitions { get; set; } = Array.Empty<IProcedure>();
        public IEnumerable<IView> ViewDefinitions { get; set; } = Array.Empty<IView>();
    }
}
