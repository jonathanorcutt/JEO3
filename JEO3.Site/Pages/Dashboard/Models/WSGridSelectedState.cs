using JEO3.Schema;

namespace JEO3.Site.Dashboard
{
    public sealed class WSGridSelectedState
    {
        public ITable Table { get; set; } = new Table();
        public IList<ITable> Tables { get; set; } = [];
        public IList<IColumn> Columns { get; set; } = [];
        public IList<IColumn> AllObjects { get; set; } = [];
        public IList<IRelationColumnPair> Relations { get; set; } = [];
        public IList<IProcedure> StoredProcedureDefinitions { get; set; } = [];
        public IList<IView> ViewDefinitions { get; set; } = [];
    }
}
